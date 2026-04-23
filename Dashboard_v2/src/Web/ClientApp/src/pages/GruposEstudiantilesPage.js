import React, { useState, useEffect, useCallback } from 'react';
import {
  Card, CardBody, CardHeader,
  Table, Button, Spinner, Alert, Badge,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Form, FormGroup, Label, Input,
} from 'reactstrap';
import Select from 'react-select';

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

export default function GruposEstudiantilesPage() {
  const [items, setItems] = useState([]);
  const [areas, setAreas] = useState([]);
  const [lineasDeInvestigacion, setLineasDeInvestigacion] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  const load = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [gruposData, areasData, lineasData] = await Promise.all([
        apiFetch('/api/GruposEstudiantiles'),
        apiFetch('/api/Areas'),
        apiFetch('/api/LineasDeInvestigacion'),
      ]);
      setItems(gruposData);
      setAreas(areasData);
      setLineasDeInvestigacion(lineasData);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { load(); }, [load]);

  function openCreate() {
    setEditing(null);
    setForm({ nombre: '', areaId: areas.length > 0 ? areas[0].id : '', lineasDeInvestigacionIds: [] });
    setFormError('');
    setModal(true);
  }

  function openEdit(item) {
    setEditing(item);
    setForm({ nombre: item.nombre, areaId: item.areaId, lineasDeInvestigacionIds: item.lineasDeInvestigacionIds ?? [] });
    setFormError('');
    setModal(true);
  }

  async function handleSave() {
    setSaving(true);
    setFormError('');
    const body = { nombre: form.nombre, areaId: form.areaId, lineasDeInvestigacionIds: form.lineasDeInvestigacionIds };
    try {
      if (editing) {
        await apiFetch(`/api/GruposEstudiantiles/${editing.id}`, { method: 'PUT', body: JSON.stringify(body) });
      } else {
        await apiFetch('/api/GruposEstudiantiles', { method: 'POST', body: JSON.stringify(body) });
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
    if (!window.confirm('¿Eliminar este grupo estudiantil?')) return;
    try {
      await apiFetch(`/api/GruposEstudiantiles/${id}`, { method: 'DELETE' });
      await load();
    } catch (e) {
      setError(e.message);
    }
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
        <h2 className="mb-0">Grupos Científicos Estudiantiles</h2>
        <div className="d-flex gap-2">
          <Button color="primary" onClick={openCreate} disabled={areas.length === 0}>
            + Nuevo grupo
          </Button>
        </div>
      </div>

      {areas.length === 0 && (
        <Alert color="warning">
          Debes crear al menos un <strong>Área</strong> antes de poder añadir grupos estudiantiles.
        </Alert>
      )}

      {error && <Alert color="danger" toggle={() => setError('')}>{error}</Alert>}

      <Card>
        <CardHeader>
          <strong>Grupos registrados</strong>
          <small className="text-muted ms-2">({items.length})</small>
        </CardHeader>
        <CardBody className="p-0">
          <Table responsive hover className="mb-0">
            <thead className="table-light">
              <tr>
                <th>Nombre</th>
                <th>Área</th>
                <th className="text-end">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {items.length === 0 && (
                <tr>
                  <td colSpan={3} className="text-center text-muted py-4">
                    No hay grupos estudiantiles registrados.
                  </td>
                </tr>
              )}
              {items.map(item => (
                <tr key={item.id}>
                  <td className="align-middle fw-semibold">{item.nombre}</td>
                  <td className="align-middle">
                    <Badge color="secondary" pill>{item.areaNombre}</Badge>
                  </td>
                  <td className="align-middle text-end">
                    <Button size="sm" color="outline-secondary" className="me-2" onClick={() => openEdit(item)}>
                      Editar
                    </Button>
                    <Button size="sm" color="outline-danger" onClick={() => handleDelete(item.id)}>
                      Eliminar
                    </Button>
                  </td>
                </tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>

      <Modal isOpen={modal} toggle={() => setModal(false)}>
        <ModalHeader toggle={() => setModal(false)}>
          {editing ? 'Editar grupo estudiantil' : 'Nuevo grupo estudiantil'}
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
    </>
  );
}
