import React, { useState, useEffect, useCallback, useRef } from 'react';
import { apiFetch } from '../utils/apiFetch';
import { useAuth } from '../contexts/AuthContext';

const ADMIN_ROLE = 'Administrator';

export default function RolesPage() {
  const { hasPermission } = useAuth();

  const [roles, setRoles]           = useState([]);
  const [allPerms, setAllPerms]     = useState([]);
  const [loading, setLoading]       = useState(false);
  const [error, setError]           = useState('');

  const [newRoleName, setNewRoleName] = useState('');
  const [creating, setCreating]       = useState(false);
  const [createError, setCreateError] = useState('');

  const [editRole, setEditRole]   = useState(null);
  const [modalOpen, setModalOpen] = useState(false);
  const [addPerm, setAddPerm]     = useState('');
  const [saving, setSaving]       = useState(false);

  const [openMenuId, setOpenMenuId] = useState(null);
  const [menuPos, setMenuPos]       = useState({ top: 0, right: 0 });
  const menuRef                     = useRef(null);

  useEffect(() => {
    const handler = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) setOpenMenuId(null);
    };
    document.addEventListener('mousedown', handler);
    document.addEventListener('scroll', () => setOpenMenuId(null), true);
    return () => {
      document.removeEventListener('mousedown', handler);
      document.removeEventListener('scroll', () => setOpenMenuId(null), true);
    };
  }, []);

  const toggleMenu = (roleId, e) => {
    e.stopPropagation();
    if (openMenuId === roleId) { setOpenMenuId(null); return; }
    const rect = e.currentTarget.getBoundingClientRect();
    setMenuPos({ top: rect.bottom + 4, right: window.innerWidth - rect.right });
    setOpenMenuId(roleId);
  };

  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [rolesRes, permsRes] = await Promise.all([
        apiFetch('/api/Users/roles'),
        apiFetch('/api/Users/system-permissions'),
      ]);
      if (!rolesRes.ok) throw new Error('No se pudieron cargar los roles.');
      if (!permsRes.ok) throw new Error('No se pudieron cargar los permisos.');
      setRoles(await rolesRes.json());
      setAllPerms(await permsRes.json());
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  // Sync editRole with fresh data after reload
  useEffect(() => {
    if (editRole && roles.length) {
      const fresh = roles.find(r => r.id === editRole.id);
      if (fresh) setEditRole(fresh);
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [roles]);

  const permLabel = (key) => allPerms.find(p => p.key === key)?.label ?? key;

  const moduleGroups = (grants) => {
    const groups = {};
    for (const g of grants) {
      const mod = allPerms.find(p => p.key === g.permission)?.module ?? 'Otros';
      if (!groups[mod]) groups[mod] = [];
      groups[mod].push(g);
    }
    return groups;
  };

  const permsByModule = allPerms.reduce((acc, p) => {
    if (!acc[p.module]) acc[p.module] = [];
    acc[p.module].push(p);
    return acc;
  }, {});

  const assignedGrants = editRole?.systemPermissions ?? [];
  const assignedKeys   = assignedGrants.map(g => g.permission);
  const availableToAdd = allPerms.filter(p => !assignedKeys.includes(p.key));

  const handleCreateRole = async (e) => {
    e.preventDefault();
    if (!newRoleName.trim()) return;
    setCreating(true);
    setCreateError('');
    try {
      const res = await apiFetch('/api/Users/roles', {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ name: newRoleName.trim() }),
      });
      if (!res.ok) {
        const data = await res.json().catch(() => ({}));
        throw new Error(data?.errors?.Name?.[0] ?? 'Error al crear el rol.');
      }
      setNewRoleName('');
      loadData();
    } catch (err) {
      setCreateError(err.message);
    } finally {
      setCreating(false);
    }
  };

  const handleDeleteRole = async (role) => {
    if (!window.confirm(`¿Eliminar el rol "${role.name}"? Esta acción es irreversible.`)) return;
    setOpenMenuId(null);
    try {
      const res = await apiFetch(`/api/Users/roles/${role.id}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('No se pudo eliminar el rol.');
      loadData();
    } catch (err) {
      alert(err.message);
    }
  };

  const openPermissions = (role) => {
    setOpenMenuId(null);
    setEditRole(role);
    setAddPerm('');
    setModalOpen(true);
  };

  const handleAssignPerm = async () => {
    if (!addPerm || !editRole) return;
    setSaving(true);
    try {
      const res = await apiFetch(`/api/Users/roles/${editRole.id}/system-permissions`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify({ permission: addPerm }),
      });
      if (!res.ok) throw new Error('Error al asignar el permiso.');
      setAddPerm('');
      await loadData();
    } catch (err) {
      alert(err.message);
    } finally {
      setSaving(false);
    }
  };

  const handleRevokePerm = async (grantId) => {
    setSaving(true);
    try {
      const res = await apiFetch(`/api/Users/roles/system-permissions/${grantId}`, { method: 'DELETE' });
      if (!res.ok) throw new Error('Error al revocar el permiso.');
      await loadData();
    } catch (err) {
      alert(err.message);
    } finally {
      setSaving(false);
    }
  };

  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header">
        <div>
          <h2 className="page-title">Roles y permisos</h2>
          <p className="page-subtitle">
            Gestiona los roles del sistema y los permisos que heredan sus usuarios.
          </p>
        </div>
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Crear nuevo rol */}
      {hasPermission('roles.create') && (
        <div className="page-card">
          <p className="page-card__label">Crear nuevo rol</p>
          <form style={{ display: 'flex', gap: '0.5rem', alignItems: 'flex-start', maxWidth: 440 }}
            onSubmit={handleCreateRole}>
            <div style={{ flex: 1 }}>
              <input
                className="form-control"
                placeholder="Nombre del rol (ej. Editor de contenidos)"
                value={newRoleName}
                onChange={e => { setNewRoleName(e.target.value); setCreateError(''); }}
                disabled={creating}
              />
              {createError && (
                <p style={{ color: '#be123c', fontSize: '0.8rem', margin: '0.25rem 0 0' }}>
                  {createError}
                </p>
              )}
            </div>
            <button className="btn btn-primary" type="submit" disabled={creating || !newRoleName.trim()}>
              {creating
                ? <span className="spinner-border spinner-border-sm" />
                : <><i className="bi bi-plus-circle me-1"></i>Crear</>}
            </button>
          </form>
        </div>
      )}

      {/* Tabla de roles */}
      <div className="table-card">
        {loading ? (
          <div className="table-placeholder">
            <span className="spinner-border spinner-border-sm me-2" />Cargando roles…
          </div>
        ) : roles.length === 0 ? (
          <div className="table-placeholder">No hay roles definidos.</div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Nombre</th>
                <th>Usuarios</th>
                <th>Permisos asignados</th>
                <th className="col-actions">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {roles.map(role => (
                <tr key={role.id}>
                  <td>
                    <strong>{role.name}</strong>
                    {role.name === ADMIN_ROLE && (
                      <span className="badge badge-blue"
                        style={{ fontSize: '0.7rem', marginLeft: '0.5rem' }}>
                        Sistema
                      </span>
                    )}
                  </td>
                  <td>
                    <span className="badge badge-gray">
                      {role.userCount} usuario{role.userCount !== 1 ? 's' : ''}
                    </span>
                  </td>
                  <td>
                    {role.name === ADMIN_ROLE ? (
                      <span className="badge badge-dark">Acceso total (implícito)</span>
                    ) : role.systemPermissions?.length > 0 ? (
                      <div className="flex-wrap-gap">
                        {Object.entries(moduleGroups(role.systemPermissions)).map(([mod, grants]) => (
                          <span key={mod} className="badge badge-blue"
                            title={grants.map(g => permLabel(g.permission)).join(', ')}>
                            {mod} ({grants.length})
                          </span>
                        ))}
                      </div>
                    ) : (
                      <span className="text-muted">Sin permisos</span>
                    )}
                  </td>
                  <td className="col-actions">
                    {(hasPermission('roles.manage_perms') || (hasPermission('roles.delete') && role.name !== ADMIN_ROLE)) && (
                    <div className="row-actions" ref={openMenuId === role.id ? menuRef : null}>
                      <button className="btn-actions-trigger" onClick={(e) => toggleMenu(role.id, e)}>
                        Acciones <i className={`bi bi-chevron-${openMenuId === role.id ? 'up' : 'down'}`}></i>
                      </button>
                      {openMenuId === role.id && (
                        <div className="row-actions__menu"
                          style={{ top: menuPos.top, right: menuPos.right }}>
                          {hasPermission('roles.manage_perms') && (
                             <button className="row-actions__item" onClick={() => openPermissions(role)}>
                               <i className="bi bi-shield-lock"></i> Editar permisos
                             </button>
                           )}
                          {hasPermission('roles.delete') && role.name !== ADMIN_ROLE && (
                            <>
                              <div className="row-actions__divider" />
                              <button
                                className="row-actions__item row-actions__item--danger"
                                onClick={() => handleDeleteRole(role)}>
                                <i className="bi bi-trash"></i> Eliminar rol
                              </button>
                            </>
                          )}
                        </div>
                      )}
                    </div>
                    )}
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* Modal editar permisos */}
      {modalOpen && editRole && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal-dialog" style={{ maxWidth: 620 }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <i className="bi bi-shield-lock me-2"></i>
                Permisos — {editRole.name}
              </h3>
              <button className="modal-close" onClick={() => setModalOpen(false)}>
                <i className="bi bi-x-lg"></i>
              </button>
            </div>

            <div className="modal-body">
              <p style={{ fontSize: '0.85rem', color: 'var(--text-secondary)', margin: 0 }}>
                Los usuarios con este rol heredan estos permisos automáticamente.
              </p>

              {editRole.name === ADMIN_ROLE ? (
                <div className="alert alert-info" style={{ marginTop: '0.5rem' }}>
                  El rol <strong>Administrator</strong> tiene acceso total de forma implícita.
                  No es necesario asignarle permisos explícitos.
                </div>
              ) : assignedGrants.length === 0 ? (
                <p className="text-muted" style={{ marginTop: '0.5rem' }}>
                  Este rol no tiene permisos asignados.
                </p>
              ) : (
                <div style={{ display: 'flex', flexDirection: 'column', gap: '1rem', marginTop: '0.5rem' }}>
                  {Object.entries(moduleGroups(assignedGrants)).map(([mod, grants]) => (
                    <div key={mod}>
                      <p style={{ fontSize: '0.78rem', fontWeight: 600, color: 'var(--text-secondary)',
                        textTransform: 'uppercase', letterSpacing: '0.05em', margin: '0 0 0.5rem' }}>
                        {mod}
                      </p>
                      <div className="flex-wrap-gap">
                        {grants.map(g => (
                          <span key={g.grantId}
                            className="badge badge-blue"
                            style={{ display: 'inline-flex', alignItems: 'center', gap: '0.3rem' }}>
                            {permLabel(g.permission)}
                            {hasPermission('roles.manage_perms') && (
                              <button
                                style={{ background: 'none', border: 'none', padding: 0,
                                  cursor: 'pointer', color: 'inherit', lineHeight: 1 }}
                                title="Revocar"
                                onClick={() => handleRevokePerm(g.grantId)}
                                disabled={saving}
                              >
                                <i className="bi bi-x" style={{ fontSize: '0.75rem' }}></i>
                              </button>
                            )}
                          </span>
                        ))}
                      </div>
                    </div>
                  ))}
                </div>
              )}

              {/* Añadir permiso */}
              {hasPermission('roles.manage_perms') && editRole.name !== ADMIN_ROLE && availableToAdd.length > 0 && (
                <div style={{ display: 'flex', gap: '0.5rem', alignItems: 'center', marginTop: '1rem',
                  paddingTop: '1rem', borderTop: '1px solid var(--border, #e5e7eb)' }}>
                  <div className="select-wrapper" style={{ flex: 1 }}>
                    <select
                      className="form-control"
                      value={addPerm}
                      onChange={e => setAddPerm(e.target.value)}
                      disabled={saving}
                    >
                      <option value="">— Seleccionar permiso —</option>
                      {Object.entries(permsByModule).map(([mod, perms]) => (
                        <optgroup key={mod} label={mod}>
                          {perms
                            .filter(p => !assignedKeys.includes(p.key))
                            .map(p => (
                              <option key={p.key} value={p.key}>{p.label}</option>
                            ))}
                        </optgroup>
                      ))}
                    </select>
                  </div>
                  <button className="btn btn-primary"
                    onClick={handleAssignPerm}
                    disabled={saving || !addPerm}>
                    {saving
                      ? <span className="spinner-border spinner-border-sm" />
                      : <><i className="bi bi-plus-circle me-1"></i>Asignar</>}
                  </button>
                </div>
              )}
            </div>

            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setModalOpen(false)}>
                Cerrar
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
