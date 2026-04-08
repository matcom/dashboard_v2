import React, { useState, useEffect, useCallback } from 'react';
import {
  Card, CardBody, CardHeader,
  Table, Button, Spinner, Alert, Badge,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Form, FormGroup, Label, Input,
} from 'reactstrap';
import Select from 'react-select';
import { useAuth } from '../contexts/AuthContext';

async function apiFetch(url, options = {}) {
  const response = await fetch(url, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...(options.headers ?? {}) },
    ...options,
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    const errors = data?.errors ?? ['Error desconocido.'];
    throw new Error(Array.isArray(errors) ? errors.join(' ') : String(errors));
  }
  return data;
}

const emptyForm = { nombre: '', areaId: '', lineasDeInvestigacionIds: [] };

export default function MisGruposDeInvestigacionPage() {
  const { user } = useAuth();
  const isJefe = user?.role === 'Jefe_de_Grupo_de_investigacion';

  const [items, setItems] = useState([]);
  const [areas, setAreas] = useState([]);
  const [lineasDeInvestigacion, setLineasDeInvestigacion] = useState([]);
  const [usuarios, setUsuarios] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  const [membrosModal, setMembrosModal] = useState(false);
  const [membrosGrupo, setMembrosGrupo] = useState(null);
  const [membrosSelected, setMembrosSelected] = useState([]);
  const [savingMiembros, setSavingMiembros] = useState(false);
  const [membrosError, setMembrosError] = useState('');
  const [membrosInput, setMembrosInput] = useState('');
  const [membrosSuggestionsOpen, setMembrosSuggestionsOpen] = useState(false);

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const gruposData = await apiFetch('/api/GruposDeInvestigacion/mine');
      setItems(gruposData);
      if (isJefe) {
        const [areasData, lineasData, usuariosData] = await Promise.all([
          apiFetch('/api/Areas'),
          apiFetch('/api/LineasDeInvestigacion'),
          apiFetch('/api/Users'),
        ]);
        setAreas(areasData);
        setLineasDeInvestigacion(lineasData);
        setUsuarios(usuariosData);
      }
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [isJefe]);

  useEffect(() => { load(); }, [load]);

  function openCreate() {
    setEditing(null);
    setForm({ nombre: '', areaId: areas.length > 0 ? areas[0].id : '', lineasDeInvestigacionIds: [] });
    setFormError('');
    setModal(true);
  }

  function openEdit(item) {
    setEditing(item);
    setForm({
      nombre: item.nombre,
      areaId: item.areaId,
      lineasDeInvestigacionIds: item.lineasDeInvestigacionIds ?? [],
    });
    setFormError('');
    setModal(true);
  }

  async function handleSave() {
    setSaving(true);
    setFormError('');
    const body = {
      nombre: form.nombre,
      areaId: form.areaId,
      lineasDeInvestigacionIds: form.lineasDeInvestigacionIds,
    };
    try {
      if (editing) {
        await apiFetch(`/api/GruposDeInvestigacion/${editing.id}`, { method: 'PUT', body: JSON.stringify(body) });
      } else {
        await apiFetch('/api/GruposDeInvestigacion', { method: 'POST', body: JSON.stringify(body) });
      }
      setModal(false);
      await load();
    } catch (e) {
      setFormError(e.message);
    } finally {
      setSaving(false);
    }
  }

  async function handleDelete(id) {
    if (!window.confirm('¿Eliminar este grupo de investigación?')) return;
    try {
      await apiFetch(`/api/GruposDeInvestigacion/${id}`, { method: 'DELETE' });
      await load();
    } catch (e) {
      setError(e.message);
    }
  }

  function openMembros(item) {
    setMembrosGrupo(item);
    setMembrosSelected(
      (item.usuariosIds ?? []).map(id => {
        const u = usuarios.find(x => x.id === id);
        return u ? { value: u.id, label: `${u.userName} (${u.email})` } : null;
      }).filter(Boolean)
    );
    setMembrosError('');
    setMembrosInput('');
    setMembrosSuggestionsOpen(false);
    setMembrosModal(true);
  }

  async function handleSaveMiembros() {
    setSavingMiembros(true);
    setMembrosError('');
    try {
      await apiFetch(`/api/GruposDeInvestigacion/${membrosGrupo.id}/miembros`, {
        method: 'PUT',
        body: JSON.stringify({ usuariosIds: membrosSelected.map(s => s.value) }),
      });
      setMembrosModal(false);
      await load();
    } catch (e) {
      setMembrosError(e.message);
    } finally {
      setSavingMiembros(false);
    }
  }

  const membrosSuggestions = membrosInput.trim().length > 0
    ? usuarios
        .filter(u =>
          !membrosSelected.some(s => s.value === u.id) &&
          `${u.userName} ${u.email}`.toLowerCase().includes(membrosInput.toLowerCase())
        )
        .slice(0, 8)
    : [];

  function addMiembro(u) {
    if (membrosSelected.some(s => s.value === u.id)) return;
    setMembrosSelected(prev => [...prev, { value: u.id, label: `${u.userName} (${u.email})` }]);
    setMembrosInput('');
    setMembrosSuggestionsOpen(false);
  }

  function removeMiembro(id) {
    if (id === user?.id) return; // cannot remove self
    setMembrosSelected(prev => prev.filter(s => s.value !== id));
  }

  if (loading) {
    return (
      <div className="d-flex justify-content-center mt-5">
        <Spinner color="primary" />
      </div>
    );
  }

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Mis Grupos de Investigación</h2>
        {isJefe && (
          <Button color="primary" onClick={openCreate} disabled={areas.length === 0}>
            + Nuevo grupo
          </Button>
        )}
      </div>

      {isJefe && areas.length === 0 && (
        <Alert color="warning">
          Debes crear al menos un <strong>Área</strong> antes de poder añadir grupos de investigación.
        </Alert>
      )}

      {error && <Alert color="danger" toggle={() => setError('')}>{error}</Alert>}

      <Card>
        <CardHeader>
          <strong>Grupos a los que perteneces</strong>
          <small className="text-muted ms-2">({items.length})</small>
        </CardHeader>
        <CardBody className="p-0">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>Nombre</th>
                <th>Área</th>
                <th>Líneas de investigación</th>
                {isJefe && <th className="text-end">Acciones</th>}
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={isJefe ? 4 : 3} className="text-center text-muted py-4">
                    No perteneces a ningún grupo de investigación.
                  </td>
                </tr>
              )}
              {items.map(item => (
                <tr key={item.id}>
                  <td className="align-middle fw-semibold">{item.nombre}</td>
                  <td className="align-middle">
                    <Badge color="secondary" pill>{item.areaNombre}</Badge>
                  </td>
                  <td className="align-middle">
                    {item.lineasDeInvestigacionIds && item.lineasDeInvestigacionIds.length > 0
                      ? lineasDeInvestigacion
                          .filter(l => item.lineasDeInvestigacionIds.includes(l.id))
                          .map(l => <Badge color="info" pill className="me-1" key={l.id}>{l.nombre}</Badge>)
                      : <span className="text-muted small">Sin líneas asignadas</span>
                    }
                  </td>
                  {isJefe && (
                    <td className="align-middle text-end">
                      <Button size="sm" color="outline-info" className="me-2" onClick={() => openMembros(item)}>
                        Miembros
                      </Button>
                      <Button size="sm" color="outline-secondary" className="me-2" onClick={() => openEdit(item)}>
                        Editar
                      </Button>
                      <Button size="sm" color="outline-danger" onClick={() => handleDelete(item.id)}>
                        Eliminar
                      </Button>
                    </td>
                  )}
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>

      {isJefe && (
        <Modal isOpen={modal} toggle={() => setModal(false)}>
          <ModalHeader toggle={() => setModal(false)}>
            {editing ? 'Editar grupo de investigación' : 'Nuevo grupo de investigación'}
          </ModalHeader>
          <ModalBody>
            {formError && <Alert color="danger">{formError}</Alert>}
            <Form>
              <FormGroup>
                <Label for="nombre">Nombre *</Label>
                <Input
                  id="nombre"
                  value={form.nombre}
                  onChange={e => setForm(f => ({ ...f, nombre: e.target.value }))}
                  placeholder="Nombre del grupo"
                />
              </FormGroup>
              <FormGroup>
                <Label for="areaId">Área *</Label>
                <Input
                  id="areaId"
                  type="select"
                  value={form.areaId}
                  onChange={e => setForm(f => ({ ...f, areaId: e.target.value }))}
                >
                  <option value="">— Selecciona un área —</option>
                  {areas.map(a => (
                    <option key={a.id} value={a.id}>{a.nombre}</option>
                  ))}
                </Input>
              </FormGroup>
              <FormGroup>
                <Label>Líneas de investigación que estudia</Label>
                <Select
                  isMulti
                  options={lineasDeInvestigacion.map(l => ({ value: l.id, label: l.nombre }))}
                  value={form.lineasDeInvestigacionIds.map(id => {
                    const l = lineasDeInvestigacion.find(x => x.id === id);
                    return l ? { value: l.id, label: l.nombre } : null;
                  }).filter(Boolean)}
                  onChange={sel => setForm(f => ({ ...f, lineasDeInvestigacionIds: sel.map(s => s.value) }))}
                  placeholder="Buscar línea..."
                  noOptionsMessage={() => 'Sin resultados'}
                  menuPortalTarget={document.body}
                  styles={{ menuPortal: base => ({ ...base, zIndex: 9999 }) }}
                />
              </FormGroup>
            </Form>
          </ModalBody>
          <ModalFooter>
            <Button color="primary" onClick={handleSave} disabled={saving}>
              {saving ? <Spinner size="sm" /> : 'Guardar'}
            </Button>
            <Button color="secondary" onClick={() => setModal(false)}>Cancelar</Button>
          </ModalFooter>
        </Modal>
      )}

      {/* Modal: Gestionar miembros */}
      {isJefe && (
        <Modal isOpen={membrosModal} toggle={() => setMembrosModal(false)} size="lg">
          <ModalHeader toggle={() => setMembrosModal(false)}>
            Miembros de «{membrosGrupo?.nombre}»
          </ModalHeader>
          <ModalBody>
            {membrosError && <Alert color="danger">{membrosError}</Alert>}
            <p className="text-muted small mb-2">
              Busca y selecciona los usuarios que forman parte de este grupo. Tú siempre permanecerás como miembro.
            </p>
            <div className="mb-2 d-flex flex-wrap gap-1">
              {membrosSelected.length === 0
                ? <span className="text-muted small">Ningún miembro seleccionado</span>
                : membrosSelected.map(s => {
                    const isLocked = s.value === user?.id;
                    return (
                      <span key={s.value}
                        className={`badge ${isLocked ? 'bg-success' : 'bg-primary'} d-inline-flex align-items-center gap-1 py-1 px-2`}
                        style={{ fontSize: '0.85rem' }}>
                        {s.label}
                        {isLocked
                          ? <small className="ms-1 opacity-75">(tú)</small>
                          : <button type="button" className="btn-close btn-close-white ms-1" style={{ fontSize: '0.55rem' }}
                              onClick={() => removeMiembro(s.value)} aria-label="Eliminar" />
                        }
                      </span>
                    );
                  })
              }
            </div>
            <div className="position-relative">
              <Input
                placeholder="Buscar usuario para agregar..."
                value={membrosInput}
                autoComplete="off"
                onChange={e => { setMembrosInput(e.target.value); setMembrosSuggestionsOpen(true); }}
                onFocus={() => { if (membrosInput.trim()) setMembrosSuggestionsOpen(true); }}
                onBlur={() => setTimeout(() => setMembrosSuggestionsOpen(false), 150)}
                onKeyDown={e => { if (e.key === 'Escape') setMembrosSuggestionsOpen(false); }}
              />
              {membrosSuggestionsOpen && membrosSuggestions.length > 0 && (
                <div className="position-absolute w-100 border rounded shadow-sm bg-white"
                  style={{ zIndex: 1000, maxHeight: 200, overflowY: 'auto', top: '100%' }}>
                  {membrosSuggestions.map(u => (
                    <div key={u.id} className="px-3 py-2"
                      style={{ cursor: 'pointer' }}
                      onMouseDown={e => { e.preventDefault(); addMiembro(u); }}
                      onMouseEnter={e => e.currentTarget.style.backgroundColor = '#f8f9fa'}
                      onMouseLeave={e => e.currentTarget.style.backgroundColor = ''}>
                      <strong>{u.userName}</strong> <span className="text-muted small">{u.email}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          </ModalBody>
          <ModalFooter>
            <Button color="primary" onClick={handleSaveMiembros} disabled={savingMiembros}>
              {savingMiembros ? <Spinner size="sm" /> : 'Guardar miembros'}
            </Button>
            <Button color="secondary" onClick={() => setMembrosModal(false)}>Cancelar</Button>
          </ModalFooter>
        </Modal>
      )}
    </>
  );
}
