import React, { useState, useEffect, useCallback, useRef } from 'react';
import { apiFetch } from '../utils/apiFetch';
import { useAuth } from '../contexts/AuthContext';

/* ─────────────────────────────────────────────
   Constants
───────────────────────────────────────────── */
const PERMISSION_LABELS = {
  read:    { label: 'Leer',     color: 'badge-blue' },
  write:   { label: 'Editar',   color: 'badge-green' },
  delete:  { label: 'Eliminar', color: 'badge-red' },
  share:   { label: 'Compartir',color: 'badge-purple' },
  approve: { label: 'Aprobar',  color: 'badge-orange' },
  admin:   { label: 'Admin',    color: 'badge-dark' },
};

// Mapa clave → etiqueta para permisos de sistema (debe coincidir con SystemPermissions.cs)
const SYSTEM_PERM_LABELS = {
  'users.view':               'Ver usuarios',
  'users.create':             'Crear usuarios',
  'users.manage':             'Gestionar usuarios (roles / estado)',
  'grants.view':              'Ver permisos de usuarios',
  'grants.system.grant':      'Asignar permisos de sistema',
  'grants.resource.grant':    'Asignar permisos de recurso',
  'grants.system.revoke':     'Revocar permisos de sistema',
  'grants.resource.revoke':   'Revocar permisos de recurso',
  'publications.view_all':    'Ver todas las publicaciones',
  'publications.create':      'Crear publicaciones',
  'publications.edit_any':    'Editar cualquier publicación',
  'publications.delete_any':  'Eliminar cualquier publicación',
  'system.all':               'Acceso completo al sistema',
};

const ALL_SYSTEM_PERMS = Object.keys(SYSTEM_PERM_LABELS);

const EMPTY_FORM  = { userName: '', email: '', password: '', roleIds: [] };
const EMPTY_GRANT = { targetUserId: '', resourceId: '', permissionName: 'read', expiresAt: '' };
const EMPTY_SYS_GRANT = { targetUserId: '', permission: '', expiresAt: '' };

/* ─────────────────────────────────────────────
   UsersPage
───────────────────────────────────────────── */
export default function UsersPage() {
  const { hasPermission } = useAuth();
  const [users, setUsers]             = useState([]);
  const [roles, setRoles]             = useState([]);
  const [loading, setLoading]         = useState(false);
  const [error, setError]             = useState(null);

  // Modales
  const [createOpen, setCreateOpen]   = useState(false);
  const [rolesOpen, setRolesOpen]     = useState(false);
  const [grantsOpen, setGrantsOpen]   = useState(false);
  const [grantOpen, setGrantOpen]     = useState(false);
  const [sysGrantsOpen, setSysGrantsOpen] = useState(false);  // ver/revocar permisos de sistema
  const [sysGrantFormOpen, setSysGrantFormOpen] = useState(false); // añadir permiso de sistema

  // Datos de modales
  const [form, setForm]               = useState(EMPTY_FORM);
  const [formErrors, setFormErrors]   = useState([]);
  const [saving, setSaving]           = useState(false);

  const [selectedUser, setSelectedUser] = useState(null);
  const [editRoles, setEditRoles]       = useState([]);

  const [grants, setGrants]               = useState([]);
  const [grantsLoading, setGrantsLoading] = useState(false);
  const [grantForm, setGrantForm]         = useState(EMPTY_GRANT);
  const [grantErrors, setGrantErrors]     = useState([]);
  const [grantSaving, setGrantSaving]     = useState(false);

  // System grants
  const [sysGrants, setSysGrants]                 = useState([]);
  const [sysGrantsLoading, setSysGrantsLoading]   = useState(false);
  const [sysGrantForm, setSysGrantForm]           = useState(EMPTY_SYS_GRANT);
  const [sysGrantErrors, setSysGrantErrors]       = useState([]);
  const [sysGrantSaving, setSysGrantSaving]       = useState(false);

  const [publications, setPublications] = useState([]);

  // Dropdown de acciones por fila
  const [openMenuId, setOpenMenuId] = useState(null);
  const [menuPos, setMenuPos] = useState({ top: 0, right: 0 });
  const menuRef = useRef(null);

  // Cerrar dropdown al hacer click fuera o al hacer scroll
  useEffect(() => {
    const handler = (e) => {
      if (menuRef.current && !menuRef.current.contains(e.target)) {
        setOpenMenuId(null);
      }
    };
    document.addEventListener('mousedown', handler);
    document.addEventListener('scroll', () => setOpenMenuId(null), true);
    return () => {
      document.removeEventListener('mousedown', handler);
      document.removeEventListener('scroll', () => setOpenMenuId(null), true);
    };
  }, []);

  const toggleMenu = (userId, e) => {
    e.stopPropagation();
    if (openMenuId === userId) {
      setOpenMenuId(null);
    } else {
      const rect = e.currentTarget.getBoundingClientRect();
      setMenuPos({ top: rect.bottom + 4, right: window.innerWidth - rect.right });
      setOpenMenuId(userId);
    }
  };

  const closeMenu = () => setOpenMenuId(null);

  /* ── Cargar datos ── */
  const loadUsers = useCallback(async () => {
    setLoading(true);
    setError(null);
    try {
      const [usersRes, rolesRes] = await Promise.all([
        apiFetch('/api/Users'),
        apiFetch('/api/Users/roles'),
      ]);
      setUsers(await usersRes.json());
      setRoles(await rolesRes.json());
    } catch {
      setError('Error al cargar usuarios.');
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadUsers(); }, [loadUsers]);

  /* ── Crear usuario ── */
  const openCreate = () => {
    setForm(EMPTY_FORM);
    setFormErrors([]);
    setCreateOpen(true);
  };

  const handleCreate = async () => {
    setSaving(true);
    setFormErrors([]);
    try {
      const res = await apiFetch('/api/Users', {
        method: 'POST',
        body: JSON.stringify(form),
      });
      if (!res.ok) {
        const err = await res.json();
        setFormErrors(err.errors
          ? Object.values(err.errors).flat()
          : [err.title ?? 'Error al crear usuario.']);
        return;
      }
      setCreateOpen(false);
      loadUsers();
    } finally {
      setSaving(false);
    }
  };

  /* ── Editar roles ── */
  const openRoles = (user) => {
    setSelectedUser(user);
    setEditRoles(user.roles.map(r => roles.find(ro => ro.name === r)?.id).filter(Boolean));
    setRolesOpen(true);
  };

  const handleSaveRoles = async () => {
    setSaving(true);
    try {
      await apiFetch(`/api/Users/${selectedUser.id}/roles`, {
        method: 'PUT',
        body: JSON.stringify({ userId: selectedUser.id, roleIds: editRoles }),
      });
      setRolesOpen(false);
      loadUsers();
    } finally {
      setSaving(false);
    }
  };

  /* ── Ver / gestionar permisos de recurso ── */
  const openGrants = async (user) => {
    setSelectedUser(user);
    setGrantsOpen(true);
    setGrantsLoading(true);
    try {
      const res = await apiFetch(`/api/Users/${user.id}/grants`);
      setGrants(await res.json());
    } finally {
      setGrantsLoading(false);
    }
  };

  const handleRevokeGrant = async (grantId) => {
    await apiFetch(`/api/Users/grants/${grantId}`, { method: 'DELETE' });
    setGrants(prev => prev.filter(g => g.grantId !== grantId));
  };

  /* ── Asignar permiso de recurso ── */
  const openGrantForm = async (user) => {
    setSelectedUser(user);
    setGrantForm({ ...EMPTY_GRANT, targetUserId: user.id });
    setGrantErrors([]);
    if (publications.length === 0) {
      const res = await apiFetch('/api/Publications?pageSize=200');
      const data = await res.json();
      setPublications(data.items ?? []);
    }
    setGrantOpen(true);
  };

  const handleSaveGrant = async () => {
    setGrantSaving(true);
    setGrantErrors([]);
    try {
      const body = {
        targetUserId: grantForm.targetUserId,
        resourceId: Number(grantForm.resourceId),
        permissionName: grantForm.permissionName,
        expiresAt: grantForm.expiresAt || null,
      };
      const res = await apiFetch('/api/Users/grants', {
        method: 'POST',
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const err = await res.json();
        setGrantErrors([err.detail ?? err.title ?? 'Error al asignar permiso.']);
        return;
      }
      setGrantOpen(false);
      if (grantsOpen && selectedUser?.id === grantForm.targetUserId) {
        const res2 = await apiFetch(`/api/Users/${grantForm.targetUserId}/grants`);
        setGrants(await res2.json());
      }
    } finally {
      setGrantSaving(false);
    }
  };

  /* ── Ver / gestionar permisos de sistema ── */
  const openSysGrants = async (user) => {
    setSelectedUser(user);
    setSysGrantsOpen(true);
    setSysGrantsLoading(true);
    try {
      const res = await apiFetch(`/api/Users/${user.id}/system-grants`);
      setSysGrants(await res.json());
    } finally {
      setSysGrantsLoading(false);
    }
  };

  const handleRevokeSysGrant = async (grantId) => {
    await apiFetch(`/api/Users/system-grants/${grantId}`, { method: 'DELETE' });
    setSysGrants(prev => prev.filter(g => g.grantId !== grantId));
  };

  /* ── Asignar permiso de sistema ── */
  const openSysGrantForm = (user) => {
    setSelectedUser(user);
    setSysGrantForm({ ...EMPTY_SYS_GRANT, targetUserId: user.id });
    setSysGrantErrors([]);
    setSysGrantFormOpen(true);
  };

  const handleSaveSysGrant = async () => {
    if (!sysGrantForm.permission) {
      setSysGrantErrors(['Selecciona un permiso.']);
      return;
    }
    setSysGrantSaving(true);
    setSysGrantErrors([]);
    try {
      const body = {
        targetUserId: sysGrantForm.targetUserId,
        permission: sysGrantForm.permission,
        expiresAt: sysGrantForm.expiresAt || null,
      };
      const res = await apiFetch('/api/Users/system-grants', {
        method: 'POST',
        body: JSON.stringify(body),
      });
      if (!res.ok) {
        const err = await res.json();
        setSysGrantErrors([err.detail ?? err.title ?? 'Error al asignar permiso de sistema.']);
        return;
      }
      setSysGrantFormOpen(false);
      // Refrescar panel de sys grants si está abierto para el mismo usuario
      if (sysGrantsOpen && selectedUser?.id === sysGrantForm.targetUserId) {
        const res2 = await apiFetch(`/api/Users/${sysGrantForm.targetUserId}/system-grants`);
        setSysGrants(await res2.json());
      }
    } finally {
      setSysGrantSaving(false);
    }
  };

  /* ── Toggle activo ── */
  const handleToggleActive = async (user) => {
    await apiFetch(`/api/Users/${user.id}/active`, {
      method: 'PUT',
      body: JSON.stringify({ isActive: !user.isActive }),
    });
    loadUsers();
  };

  const set  = (field) => (e) => setForm(f => ({ ...f, [field]: e.target.value }));
  const setG = (field) => (e) => setGrantForm(f => ({ ...f, [field]: e.target.value }));
  const setSG = (field) => (e) => setSysGrantForm(f => ({ ...f, [field]: e.target.value }));

  const toggleCheckRole = (id) =>
    setForm(f => ({ ...f, roleIds: f.roleIds.includes(id) ? f.roleIds.filter(r => r !== id) : [...f.roleIds, id] }));

  const toggleEditRole = (id) =>
    setEditRoles(prev => prev.includes(id) ? prev.filter(r => r !== id) : [...prev, id]);

  /* ─────────────────────────────────────────────
     Render
  ───────────────────────────────────────────── */
  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header">
        <div>
          <h2 className="page-title">Usuarios</h2>
          <p className="page-subtitle">Gestión de cuentas de usuario y asignación de permisos</p>
        </div>
        {hasPermission('users.create') && (
          <button className="btn btn-primary" onClick={openCreate}>
            <i className="bi bi-person-plus me-1"></i> Nuevo usuario
          </button>
        )}
      </div>

      {error && <div className="alert alert-danger">{error}</div>}

      {/* Tabla */}
      <div className="table-card">
        {loading ? (
          <div className="table-placeholder">Cargando…</div>
        ) : users.length === 0 ? (
          <div className="table-placeholder">No hay usuarios registrados.</div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Usuario</th>
                <th>Email</th>
                <th>Roles</th>
                <th>Estado</th>
                <th>Creado</th>
                <th className="col-actions" style={{ width: 200 }}>Acciones</th>
              </tr>
            </thead>
            <tbody>
              {users.map(u => (
                <tr key={u.id}>
                  <td><strong>{u.userName}</strong></td>
                  <td className="col-author">{u.email}</td>
                  <td>
                    <div className="flex-wrap-gap">
                      {u.roles.length > 0
                        ? u.roles.map(r => <span key={r} className="badge badge-gray">{r}</span>)
                        : <span className="text-muted">Sin rol</span>}
                    </div>
                  </td>
                  <td>
                    <span className={`badge ${u.isActive ? 'badge-green' : 'badge-red'}`}>
                      {u.isActive ? 'Activo' : 'Inactivo'}
                    </span>
                  </td>
                  <td className="col-author">{new Date(u.createdAt).toLocaleDateString('es-ES')}</td>
                  <td className="col-actions">
                    {(hasPermission('users.manage') || hasPermission('grants.view') || hasPermission('grants.resource.grant')) && (
                    <div className="row-actions" ref={openMenuId === u.id ? menuRef : null}>
                      <button
                        className="btn-actions-trigger"
                        onClick={(e) => toggleMenu(u.id, e)}
                      >
                        Acciones <i className={`bi bi-chevron-${openMenuId === u.id ? 'up' : 'down'}`}></i>
                      </button>

                      {openMenuId === u.id && (
                        <div className="row-actions__menu" style={{ top: menuPos.top, right: menuPos.right }}>
                          {hasPermission('users.manage') && (
                            <button className="row-actions__item" onClick={() => { closeMenu(); openRoles(u); }}>
                              <i className="bi bi-shield-lock"></i> Editar roles
                            </button>
                          )}
                          {(hasPermission('grants.view') || hasPermission('grants.resource.grant')) && (
                            <div className="row-actions__divider" />
                          )}
                          {hasPermission('grants.view') && (
                            <button className="row-actions__item" onClick={() => { closeMenu(); openGrants(u); }}>
                              <i className="bi bi-key"></i> Ver permisos de recurso
                            </button>
                          )}
                          {hasPermission('grants.resource.grant') && (
                            <button className="row-actions__item" onClick={() => { closeMenu(); openGrantForm(u); }}>
                              <i className="bi bi-plus-circle"></i> Asignar perm. recurso
                            </button>
                          )}
                          {hasPermission('grants.view') && (
                            <button className="row-actions__item" onClick={() => { closeMenu(); openSysGrants(u); }}>
                              <i className="bi bi-cpu"></i> Permisos del sistema
                            </button>
                          )}
                          {hasPermission('users.manage') && (
                            <>
                              <div className="row-actions__divider" />
                              <button
                                className={`row-actions__item${u.isActive ? ' row-actions__item--danger' : ''}`}
                                onClick={() => { closeMenu(); handleToggleActive(u); }}
                              >
                                <i className={`bi bi-${u.isActive ? 'person-x' : 'person-check'}`}></i>
                                {u.isActive ? 'Desactivar usuario' : 'Activar usuario'}
                              </button>
                            </>
                          )}
                        </div>
                      )}
                    </div>                    )}                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        )}
      </div>

      {/* ── Modal: Crear usuario ── */}
      {createOpen && (
        <div className="modal-overlay" onClick={() => setCreateOpen(false)}>
          <div className="modal-dialog" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Nuevo usuario</h3>
              <button className="modal-close" onClick={() => setCreateOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body">
              {formErrors.length > 0 && (
                <div className="alert alert-danger">
                  <ul className="mb-0 ps-3">{formErrors.map((e, i) => <li key={i}>{e}</li>)}</ul>
                </div>
              )}
              <div className="form-group">
                <label className="form-label">Nombre de usuario <span className="required">*</span></label>
                <input className="form-control" value={form.userName} onChange={set('userName')} placeholder="usuario123" />
              </div>
              <div className="form-group">
                <label className="form-label">Email <span className="required">*</span></label>
                <input className="form-control" type="email" value={form.email} onChange={set('email')} placeholder="usuario@dominio.com" />
              </div>
              <div className="form-group">
                <label className="form-label">Contraseña <span className="required">*</span></label>
                <input className="form-control" type="password" value={form.password} onChange={set('password')} placeholder="Mínimo 6 caracteres" />
              </div>
              <div className="form-section">
                <span className="form-label">Roles</span>
                {roles.map(r => (
                  <label key={r.id} className="form-check">
                    <input type="checkbox" checked={form.roleIds.includes(r.id)} onChange={() => toggleCheckRole(r.id)} />
                    <span>{r.name}</span>
                  </label>
                ))}
                {roles.length === 0 && <span className="text-muted">No hay roles disponibles.</span>}
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setCreateOpen(false)} disabled={saving}>Cancelar</button>
              <button className="btn btn-primary"   onClick={handleCreate}              disabled={saving}>
                {saving ? 'Creando…' : 'Crear usuario'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Modal: Editar roles ── */}
      {rolesOpen && selectedUser && (
        <div className="modal-overlay" onClick={() => setRolesOpen(false)}>
          <div className="modal-dialog modal-dialog--sm" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Roles de <em>{selectedUser.userName}</em></h3>
              <button className="modal-close" onClick={() => setRolesOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body">
              {roles.map(r => (
                <label key={r.id} className="form-check">
                  <input type="checkbox" checked={editRoles.includes(r.id)} onChange={() => toggleEditRole(r.id)} />
                  <span>{r.name}</span>
                </label>
              ))}
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setRolesOpen(false)} disabled={saving}>Cancelar</button>
              <button className="btn btn-primary"   onClick={handleSaveRoles}           disabled={saving}>
                {saving ? 'Guardando…' : 'Guardar'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Modal: Ver permisos de recurso ── */}
      {grantsOpen && selectedUser && (
        <div className="modal-overlay" onClick={() => setGrantsOpen(false)}>
          <div className="modal-dialog" style={{ maxWidth: 680 }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Permisos de recurso — <em>{selectedUser.userName}</em></h3>
              <button className="modal-close" onClick={() => setGrantsOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body" style={{ padding: '0.75rem 1.25rem' }}>
              {grantsLoading ? (
                <p className="text-muted">Cargando permisos…</p>
              ) : grants.length === 0 ? (
                <p className="text-muted">No hay permisos de recurso asignados.</p>
              ) : (
                <table className="data-table">
                  <thead>
                    <tr><th>Recurso</th><th>Tipo</th><th>Permiso</th><th>Expira</th><th></th></tr>
                  </thead>
                  <tbody>
                    {grants.map(g => (
                      <tr key={g.grantId}>
                        <td style={{ maxWidth: 220 }}>{g.resourceTitle}</td>
                        <td><span className="badge badge-gray">{g.resourceType}</span></td>
                        <td>
                          <span className={`badge ${PERMISSION_LABELS[g.permissionName]?.color ?? 'badge-gray'}`}>
                            {PERMISSION_LABELS[g.permissionName]?.label ?? g.permissionName}
                          </span>
                        </td>
                        <td className="col-author">{g.expiresAt ? new Date(g.expiresAt).toLocaleDateString('es-ES') : 'Sin límite'}</td>
                        <td style={{ textAlign: 'right' }}>
                          {hasPermission('grants.resource.revoke') && (
                            <button className="btn-icon btn-icon--delete" title="Revocar" onClick={() => handleRevokeGrant(g.grantId)}>
                              <i className="bi bi-trash"></i>
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setGrantsOpen(false)}>Cerrar</button>
              {hasPermission('grants.resource.grant') && (
                <button className="btn btn-primary" onClick={() => { setGrantsOpen(false); openGrantForm(selectedUser); }}>
                  <i className="bi bi-plus-lg me-1"></i> Asignar permiso
                </button>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ── Modal: Asignar permiso de recurso ── */}
      {grantOpen && selectedUser && (
        <div className="modal-overlay" onClick={() => setGrantOpen(false)}>
          <div className="modal-dialog modal-dialog--sm" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Asignar permiso de recurso a <em>{selectedUser.userName}</em></h3>
              <button className="modal-close" onClick={() => setGrantOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body">
              {grantErrors.length > 0 && (
                <div className="alert alert-danger">
                  {grantErrors.map((e, i) => <p key={i} className="mb-0">{e}</p>)}
                </div>
              )}
              <div className="form-group">
                <label className="form-label">Publicación <span className="required">*</span></label>
                <div className="select-wrapper">
                  <select className="form-control" value={grantForm.resourceId} onChange={setG('resourceId')}>
                    <option value="">— Seleccionar publicación —</option>
                    {publications.map(p => (
                      <option key={p.id} value={p.resourceId}>{p.title}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Permiso <span className="required">*</span></label>
                <div className="select-wrapper">
                  <select className="form-control" value={grantForm.permissionName} onChange={setG('permissionName')}>
                    {Object.entries(PERMISSION_LABELS).map(([key, val]) => (
                      <option key={key} value={key}>{val.label}</option>
                    ))}
                  </select>
                </div>
              </div>
              <div className="form-group">
                <label className="form-label">Fecha de expiración <span className="text-muted">(opcional)</span></label>
                <input className="form-control" type="datetime-local" value={grantForm.expiresAt} onChange={setG('expiresAt')} />
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setGrantOpen(false)}  disabled={grantSaving}>Cancelar</button>
              <button className="btn btn-primary"   onClick={handleSaveGrant}           disabled={grantSaving || !grantForm.resourceId}>
                {grantSaving ? 'Asignando…' : 'Asignar permiso'}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Modal: Permisos de sistema del usuario ── */}
      {sysGrantsOpen && selectedUser && (
        <div className="modal-overlay" onClick={() => setSysGrantsOpen(false)}>
          <div className="modal-dialog" style={{ maxWidth: 680 }} onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <i className="bi bi-cpu me-2"></i>
                Permisos del sistema — <em>{selectedUser.userName}</em>
              </h3>
              <button className="modal-close" onClick={() => setSysGrantsOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body" style={{ padding: '0.75rem 1.25rem' }}>
              <p className="text-muted" style={{ marginBottom: '0.75rem' }}>
                Los permisos del sistema controlan qué acciones puede realizar este usuario en la plataforma
                (crear usuarios, gestionar permisos, ver/editar publicaciones, etc.).
                Los usuarios con rol <strong>Administrator</strong> tienen acceso completo
                sin necesitar grants explícitos.
              </p>
              {sysGrantsLoading ? (
                <p className="text-muted">Cargando…</p>
              ) : sysGrants.length === 0 ? (
                <p className="text-muted">No hay permisos de sistema asignados explícitamente a este usuario.</p>
              ) : (
                <table className="data-table">
                  <thead>
                    <tr><th>Permiso</th><th>Clave</th><th>Expira</th><th></th></tr>
                  </thead>
                  <tbody>
                    {sysGrants.map(g => (
                      <tr key={g.grantId}>
                        <td>{SYSTEM_PERM_LABELS[g.permission] ?? g.permission}</td>
                        <td><code style={{ fontSize: '0.78rem', color: '#6b7280' }}>{g.permission}</code></td>
                        <td className="col-author">{g.expiresAt ? new Date(g.expiresAt).toLocaleDateString('es-ES') : 'Sin límite'}</td>
                        <td style={{ textAlign: 'right' }}>
                          {hasPermission('grants.system.revoke') && (
                            <button className="btn-icon btn-icon--delete" title="Revocar" onClick={() => handleRevokeSysGrant(g.grantId)}>
                              <i className="bi bi-trash"></i>
                            </button>
                          )}
                        </td>
                      </tr>
                    ))}
                  </tbody>
                </table>
              )}
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setSysGrantsOpen(false)}>Cerrar</button>
              {hasPermission('grants.system.grant') && (
                <button className="btn btn-primary" onClick={() => { setSysGrantsOpen(false); openSysGrantForm(selectedUser); }}>
                  <i className="bi bi-plus-lg me-1"></i> Asignar permiso del sistema
                </button>
              )}
            </div>
          </div>
        </div>
      )}

      {/* ── Modal: Asignar permiso de sistema ── */}
      {sysGrantFormOpen && selectedUser && (
        <div className="modal-overlay" onClick={() => setSysGrantFormOpen(false)}>
          <div className="modal-dialog modal-dialog--sm" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                <i className="bi bi-cpu me-2"></i>
                Asignar permiso del sistema a <em>{selectedUser.userName}</em>
              </h3>
              <button className="modal-close" onClick={() => setSysGrantFormOpen(false)}><i className="bi bi-x-lg"></i></button>
            </div>
            <div className="modal-body">
              {sysGrantErrors.length > 0 && (
                <div className="alert alert-danger">
                  {sysGrantErrors.map((e, i) => <p key={i} className="mb-0">{e}</p>)}
                </div>
              )}
              <div className="form-group">
                <label className="form-label">Permiso <span className="required">*</span></label>
                <div className="select-wrapper">
                  <select className="form-control" value={sysGrantForm.permission} onChange={setSG('permission')}>
                    <option value="">— Seleccionar permiso —</option>
                    {ALL_SYSTEM_PERMS.map(p => (
                      <option key={p} value={p}>{SYSTEM_PERM_LABELS[p]}</option>
                    ))}
                  </select>
                </div>
                {sysGrantForm.permission && (
                  <p className="text-muted" style={{ marginTop: '0.35rem' }}>
                    <code>{sysGrantForm.permission}</code>
                  </p>
                )}
              </div>
              <div className="form-group">
                <label className="form-label">Fecha de expiración <span className="text-muted">(opcional)</span></label>
                <input className="form-control" type="datetime-local" value={sysGrantForm.expiresAt} onChange={setSG('expiresAt')} />
              </div>
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setSysGrantFormOpen(false)} disabled={sysGrantSaving}>Cancelar</button>
              <button className="btn btn-primary"   onClick={handleSaveSysGrant}              disabled={sysGrantSaving || !sysGrantForm.permission}>
                {sysGrantSaving ? 'Asignando…' : 'Asignar permiso'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
