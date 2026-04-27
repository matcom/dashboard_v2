import React, { useState, useEffect, useCallback } from 'react';
import {
  Card, CardBody, CardHeader,
  Table, Button, Spinner, Alert,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Form, FormGroup, Label, Input,
} from 'reactstrap';

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

const emptyForm = { titulo: '', numeroSolicitudConcesion: '', esNacional: false };

export default function PatentesPage() {
  const [items, setItems] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null);
  const [form, setForm] = useState(emptyForm);
  const [saving, setSaving] = useState(false);
  const [formError, setFormError] = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setItems(await apiFetch('/api/Patentes')); } catch (e) { setError(e.message); } finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  function openCreate() { setEditing(null); setForm(emptyForm); setFormError(''); setModal(true); }
  function openEdit(it) { setEditing(it); setForm({ titulo: it.titulo, numeroSolicitudConcesion: it.numeroSolicitudConcesion, esNacional: it.esNacional }); setFormError(''); setModal(true); }

  async function handleSave(e) {
    if (e) e.preventDefault(); setSaving(true); setFormError('');
    const body = { titulo: form.titulo, numeroSolicitudConcesion: form.numeroSolicitudConcesion, esNacional: form.esNacional };
    try {
      if (editing) await apiFetch(`/api/Patentes/${editing.id}`, { method: 'PUT', body: JSON.stringify(body) });
      else await apiFetch('/api/Patentes', { method: 'POST', body: JSON.stringify(body) });
      setModal(false); await load();
    } catch (e) { setFormError(e.message); } finally { setSaving(false); }
  }

  async function handleDelete(id) { if (!window.confirm('¿Eliminar esta patente?')) return; try { await apiFetch(`/api/Patentes/${id}`, { method: 'DELETE' }); await load(); } catch (e) { setError(e.message); } }

  if (loading) return <div className="d-flex justify-content-center mt-5"><Spinner color="primary" /></div>;

  return (
    <>
      <div className="d-flex justify-content-between align-items-center mb-4">
        <h2 className="mb-0">Patentes</h2>
        <Button color="primary" onClick={openCreate}>+ Nueva patente</Button>
      </div>

      {error && <Alert color="danger">{error}</Alert>}

      <Card>
        <CardHeader><strong>Patentes</strong> <small className="text-muted ms-2">({items.length})</small></CardHeader>
        <CardBody className="p-0">
          <Table responsive hover className="mb-0">
            <thead className="table-light"><tr><th>Título</th><th>Número solicitud</th><th>Nacional</th><th className="text-end">Acciones</th></tr></thead>
            <tbody>
              {items.length === 0 && <tr><td colSpan={4} className="text-center text-muted py-4">No hay patentes.</td></tr>}
              {items.map(i => (
                <tr key={i.id}><td>{i.titulo}</td><td>{i.numeroSolicitudConcesion}</td><td>{i.esNacional ? 'Sí' : 'No'}</td>
                  <td className="text-end">
                    <Button size="sm" color="outline-secondary" className="me-2" onClick={() => openEdit(i)}>Editar</Button>
                    <Button size="sm" color="outline-danger" onClick={() => handleDelete(i.id)}>Eliminar</Button>
                  </td></tr>
              ))}
            </tbody>
          </Table>
        </CardBody>
      </Card>

      <Modal isOpen={modal} toggle={() => setModal(false)}>
        <Form onSubmit={handleSave}>
          <ModalHeader toggle={() => setModal(false)}>{editing ? 'Editar patente' : 'Nueva patente'}</ModalHeader>
          <ModalBody>
            {formError && <Alert color="danger">{formError}</Alert>}
            <FormGroup>
              <Label>Título *</Label>
              <Input value={form.titulo} onChange={e => setForm(f => ({ ...f, titulo: e.target.value }))} />
            </FormGroup>
            <FormGroup>
              <Label>Número solicitud/concesión *</Label>
              <Input value={form.numeroSolicitudConcesion} onChange={e => setForm(f => ({ ...f, numeroSolicitudConcesion: e.target.value }))} />
            </FormGroup>
            <FormGroup check>
              <Label check>
                <Input type="checkbox" checked={form.esNacional} onChange={e => setForm(f => ({ ...f, esNacional: e.target.checked }))} />{' '}
                Es nacional
              </Label>
            </FormGroup>
          </ModalBody>
          <ModalFooter>
            <Button color="primary" type="submit" disabled={saving}>{saving ? <Spinner size="sm" /> : 'Guardar'}</Button>
            <Button color="secondary" onClick={() => setModal(false)}>Cancelar</Button>
          </ModalFooter>
        </Form>
      </Modal>
    </>
  );
}
