import React, { useState, useEffect, useCallback, useRef } from 'react';
import {
  Card, CardBody, CardHeader,
  Table, Button, Badge,
  Spinner, Alert,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Form, FormGroup, Label, Input,
} from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';

async function apiFetch(url, options = {}) {
  const response = await fetch(url, {
    credentials: 'include',
    headers: { 'Content-Type': 'application/json', ...(options.headers ?? {}) },
    ...options,
  });
  const data = await response.json().catch(() => null);
  if (!response.ok) {
    let message;
    if (data?.errors) {
      // { errors: string[] }  — nuestro formato
      // { errors: { Field: string[] } } — ProblemDetails con validación
      const errs = Array.isArray(data.errors)
        ? data.errors
        : Object.values(data.errors).flat();
      message = errs.join(' ');
    } else if (data?.title) {
      // ProblemDetails estándar de ASP.NET Core
      message = data.title;
    } else {
      message = `Error ${response.status}`;
    }
    throw new Error(message);
  }
  return data;
}

const EMPTY_FORM = {
  title: '',
  publicationData: '',
  urlDoi: '',
  publicationTypeId: '',
};

export default function PublicationsPage() {
  const { user } = useAuth();
  const [publications, setPublications] = useState([]);
  const [types, setTypes] = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Modal de crear / editar
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null); // null = crear, objeto = editar
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState('');
  const [formLoading, setFormLoading] = useState(false);

  // Modal de confirmación de borrado
  const [deleteModal, setDeleteModal] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteError, setDeleteError] = useState('');
  // Creación inline de nuevo tipo
  const [newTypeName, setNewTypeName] = useState('');
  const [showNewType, setShowNewType] = useState(false);
  const [newTypeLoading, setNewTypeLoading] = useState(false);
  const [newTypeError, setNewTypeError] = useState('');

  // Tag-picker de coautores
  const [coauthorTags, setCoauthorTags] = useState([]); // [{id?, name}]
  const [coauthorInput, setCoauthorInput] = useState('');
  const [coauthorSuggestions, setCoauthorSuggestions] = useState([]);
  const [suggestionsOpen, setSuggestionsOpen] = useState(false);
  const coauthorDebounce = useRef(null);
  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [pubs, pubTypes] = await Promise.all([
        apiFetch('/api/Publications'),
        apiFetch('/api/Publications/types'),
      ]);
      setPublications(pubs);
      setTypes(pubTypes);
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  // ── helpers de formulario ──────────────────────────────────────────────────

  function openCreate() {
    setEditing(null);
    setForm({ ...EMPTY_FORM, publicationTypeId: types[0]?.id ?? '' });
    setFormError('');
    setShowNewType(false);
    setNewTypeName('');
    setNewTypeError('');
    setCoauthorTags([]);
    setCoauthorInput('');
    setSuggestionsOpen(false);
    setModal(true);
  }

  function openEdit(pub) {
    setEditing(pub);
    setForm({
      title: pub.title,
      publicationData: pub.publicationData,
      urlDoi: pub.urlDoi ?? '',
      publicationTypeId: pub.publicationType.id,
    });
    setFormError('');
    setShowNewType(false);
    setNewTypeName('');
    setNewTypeError('');
    // Pre-cargar coautores (todos excepto el usuario actual)
    const initialTags = (pub.authors ?? [])
      .filter(a => a.userId !== user?.id)
      .map(a => ({
        id: a.id,
        name: a.name,
        type: a.userId ? 'author' : 'author',  // author existente (linked o free)
      }));
    setCoauthorTags(initialTags);
    setCoauthorInput('');
    setSuggestionsOpen(false);
    setModal(true);
  }

  function handleFormChange(e) {
    setForm(f => ({ ...f, [e.target.name]: e.target.value }));
  }

  async function handleSubmit() {
    if (!form.title.trim() || !form.publicationTypeId) {
      setFormError('El título y el tipo de publicación son obligatorios.');
      return;
    }
    setFormLoading(true);
    setFormError('');
    try {
      if (editing) {
        // PUT — actualizar
        await apiFetch(`/api/Publications/${editing.id}`, {
          method: 'PUT',
          body: JSON.stringify({
            title: form.title,
            publicationData: form.publicationData,
            publicationTypeId: form.publicationTypeId,
            urlDoi: form.urlDoi || null,
            additionalAuthorIds: coauthorTags.filter(t => t.type === 'author').map(t => t.id),
            additionalAuthorNames: coauthorTags.filter(t => !t.type).map(t => t.name),
            additionalUserIds: coauthorTags.filter(t => t.type === 'user').map(t => t.id),
          }),
        });
      } else {
        // POST — crear
        await apiFetch('/api/Publications', {
          method: 'POST',
          body: JSON.stringify({
            title: form.title,
            publicationData: form.publicationData,
            publicationTypeId: form.publicationTypeId,
            urlDoi: form.urlDoi || null,
            additionalAuthorIds: coauthorTags.filter(t => t.type === 'author').map(t => t.id),
            additionalAuthorNames: coauthorTags.filter(t => !t.type).map(t => t.name),
            additionalUserIds: coauthorTags.filter(t => t.type === 'user').map(t => t.id),
          }),
        });
      }
      setModal(false);
      loadData();
    } catch (e) {
      setFormError(e.message);
    } finally {
      setFormLoading(false);
    }
  }

  // ── tag-picker de coautores ─────────────────────────────────────────────────────

  function addCoauthor(tag) {
    if (!tag.name.trim()) return;
    const isDup = coauthorTags.some(t =>
      (tag.id && t.id === tag.id) || t.name.toLowerCase() === tag.name.trim().toLowerCase()
    );
    if (!isDup) setCoauthorTags(prev => [...prev, { id: tag.id, name: tag.name.trim(), type: tag.type }]);
    setCoauthorInput('');
    setCoauthorSuggestions([]);
    setSuggestionsOpen(false);
  }

  function removeCoauthor(index) {
    setCoauthorTags(prev => prev.filter((_, i) => i !== index));
  }

  function handleCoauthorInput(e) {
    const val = e.target.value;
    if (val.endsWith(',')) {
      const name = val.slice(0, -1).trim();
      if (name) addCoauthor({ name });
      return;
    }
    setCoauthorInput(val);
    clearTimeout(coauthorDebounce.current);
    if (val.trim().length >= 2) {
      coauthorDebounce.current = setTimeout(async () => {
        try {
          const res = await fetch(`/api/Authors/search-coauthors?q=${encodeURIComponent(val.trim())}`, { credentials: 'include' });
          if (res.ok) {
            const data = await res.json();
            setCoauthorSuggestions(data);
            setSuggestionsOpen(data.length > 0);
          }
        } catch { /* ignorar */ }
      }, 250);
    } else {
      setCoauthorSuggestions([]);
      setSuggestionsOpen(false);
    }
  }

  function handleCoauthorKeyDown(e) {
    if ((e.key === 'Enter' || e.key === ',') && coauthorInput.trim()) {
      e.preventDefault();
      addCoauthor({ name: coauthorInput.trim() });
    }
    if (e.key === 'Escape') setSuggestionsOpen(false);
  }

  // ── creación de tipo inline ────────────────────────────────────────────────

  async function handleCreateType() {
    if (!newTypeName.trim()) return;
    setNewTypeLoading(true);
    setNewTypeError('');
    try {
      const created = await apiFetch('/api/Publications/types', {
        method: 'POST',
        body: JSON.stringify({ name: newTypeName.trim() }),
      });
      setTypes(prev => [...prev, created].sort((a, b) => a.name.localeCompare(b.name)));
      setForm(f => ({ ...f, publicationTypeId: created.id }));
      setShowNewType(false);
      setNewTypeName('');
    } catch (e) {
      setNewTypeError(e.message);
    } finally {
      setNewTypeLoading(false);
    }
  }

  // ── borrado ────────────────────────────────────────────────────────────────

  function openDelete(pub) {
    setToDelete(pub);
    setDeleteError('');
    setDeleteModal(true);
  }

  async function handleDelete() {
    if (!toDelete) return;
    setDeleteLoading(true);
    setDeleteError('');
    try {
      await apiFetch(`/api/Publications/${toDelete.id}`, { method: 'DELETE' });
      setDeleteModal(false);
      loadData();
    } catch (e) {
      setDeleteError(e.message);
    } finally {
      setDeleteLoading(false);
    }
  }

  // ── render ─────────────────────────────────────────────────────────────────

  return (
    <>
      <Card className="shadow-sm">
        <CardHeader className="d-flex justify-content-between align-items-center">
          <strong>Mis publicaciones</strong>
          <Button color="primary" size="sm" onClick={openCreate} disabled={loading}>
            <i className="bi bi-plus-lg me-1"></i> Nueva publicación
          </Button>
        </CardHeader>
        <CardBody>
          {loading && <div className="text-center py-4"><Spinner /></div>}
          {error && <Alert color="danger">{error}</Alert>}

          {!loading && !error && publications.length === 0 && (
            <p className="text-muted text-center py-3">
              Aún no tienes publicaciones registradas.
            </p>
          )}

          {!loading && publications.length > 0 && (
            <Table hover responsive size="sm">
              <thead>
                <tr>
                  <th>Título</th>
                  <th>Tipo</th>
                  <th>URL / DOI</th>
                  <th>Autores</th>
                  <th style={{ width: 130 }}></th>
                </tr>
              </thead>
              <tbody>
                {publications.map(pub => (
                  <tr key={pub.id}>
                    <td>{pub.title}</td>
                    <td>
                      <Badge color="secondary" pill>
                        {pub.publicationType.name}
                      </Badge>
                    </td>
                    <td style={{ maxWidth: 200 }}>
                      {pub.urlDoi
                        ? <a href={pub.urlDoi} target="_blank" rel="noopener noreferrer"
                             className="text-truncate d-block" style={{ maxWidth: 190 }}
                             title={pub.urlDoi}>{pub.urlDoi}</a>
                        : <span className="text-muted">—</span>}
                    </td>
                    <td>
                      {pub.authors.map(a => (
                        <span key={a.id} className="me-2">
                          {a.name}
                          {a.userId && (
                            <i className="bi bi-person-check ms-1 text-success"
                               title="Usuario registrado"></i>
                          )}
                        </span>
                      ))}
                    </td>
                    <td className="text-end">
                      <Button
                        color="outline-primary"
                        size="sm"
                        className="me-1"
                        onClick={() => openEdit(pub)}
                      >
                        <i className="bi bi-pencil"></i>
                      </Button>
                      <Button
                        color="outline-danger"
                        size="sm"
                        onClick={() => openDelete(pub)}
                      >
                        <i className="bi bi-trash"></i>
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </CardBody>
      </Card>

      {/* ── Modal crear / editar ── */}
      <Modal isOpen={modal} toggle={() => setModal(false)} size="lg">
        <ModalHeader toggle={() => setModal(false)}>
          {editing ? 'Editar publicación' : 'Nueva publicación'}
        </ModalHeader>
        <ModalBody>
          {formError && <Alert color="danger">{formError}</Alert>}
          <Form>
            <FormGroup>
              <Label for="title">Título <span className="text-danger">*</span></Label>
              <Input
                id="title"
                name="title"
                value={form.title}
                onChange={handleFormChange}
                placeholder="Título de la publicación"
              />
            </FormGroup>
            <FormGroup>
              <div className="d-flex justify-content-between align-items-center mb-1">
                <Label for="publicationTypeId" className="mb-0">
                  Tipo <span className="text-danger">*</span>
                </Label>
                {!showNewType && (
                  <button type="button" className="btn btn-link btn-sm p-0"
                    onClick={() => { setShowNewType(true); setNewTypeError(''); }}>
                    <i className="bi bi-plus-circle me-1"></i>Nuevo tipo
                  </button>
                )}
              </div>
              {showNewType ? (
                <>
                  <div className="input-group input-group-sm">
                    <Input
                      value={newTypeName}
                      onChange={e => setNewTypeName(e.target.value)}
                      placeholder="Nombre del nuevo tipo"
                      onKeyDown={e => e.key === 'Enter' && handleCreateType()}
                      disabled={newTypeLoading}
                      autoFocus
                    />
                    <Button color="primary" size="sm" onClick={handleCreateType}
                      disabled={newTypeLoading || !newTypeName.trim()}>
                      {newTypeLoading ? <Spinner size="sm" /> : 'Crear'}
                    </Button>
                    <Button color="secondary" outline size="sm"
                      onClick={() => { setShowNewType(false); setNewTypeName(''); setNewTypeError(''); }}
                      disabled={newTypeLoading}>
                      Cancelar
                    </Button>
                  </div>
                  {newTypeError && <small className="text-danger">{newTypeError}</small>}
                </>
              ) : (
                <Input
                  type="select"
                  id="publicationTypeId"
                  name="publicationTypeId"
                  value={form.publicationTypeId}
                  onChange={handleFormChange}
                >
                  {types.map(t => (
                    <option key={t.id} value={t.id}>{t.name}</option>
                  ))}
                </Input>
              )}
            </FormGroup>
            <FormGroup>
              <Label for="urlDoi">URL / DOI</Label>
              <Input
                id="urlDoi"
                name="urlDoi"
                value={form.urlDoi}
                onChange={handleFormChange}
                placeholder="https://doi.org/10.xxxx/... o URL de la publicación"
              />
            </FormGroup>
            <FormGroup>
              <Label for="publicationData">Datos / Resumen</Label>
              <Input
                type="textarea"
                id="publicationData"
                name="publicationData"
                rows={4}
                value={form.publicationData}
                onChange={handleFormChange}
                placeholder="Resumen o cualquier información adicional relevante"
              />
            </FormGroup>
            {/* Coautores */}
            {(
              <FormGroup>
                <Label>Coautores adicionales</Label>
                {/* Tags de coautores ya añadidos */}
                {coauthorTags.length > 0 && (
                  <div className="d-flex flex-wrap gap-1 mb-2">
                    {coauthorTags.map((t, i) => (
                      <span
                        key={i}
                        className={`badge d-inline-flex align-items-center gap-1 ${
                          t.type === 'author' ? 'bg-primary'
                          : t.type === 'user' ? 'bg-success'
                          : 'bg-secondary'
                        }`}
                        style={{ fontSize: '0.85em' }}
                      >
                        {t.type === 'author' && <i className="bi bi-person-check" title="Autor existente"></i>}
                        {t.type === 'user' && <i className="bi bi-person-plus" title="Usuario del sistema (se creará como autor)"></i>}
                        {t.name}
                        <button
                          type="button"
                          onClick={() => removeCoauthor(i)}
                          style={{ background: 'none', border: 'none', padding: '0 0 0 4px', color: 'inherit', cursor: 'pointer', lineHeight: 1 }}
                          aria-label="Quitar"
                        >×</button>
                      </span>
                    ))}
                  </div>
                )}
                {/* Input con dropdown */}
                <div style={{ position: 'relative' }}>
                  <Input
                    value={coauthorInput}
                    onChange={handleCoauthorInput}
                    onKeyDown={handleCoauthorKeyDown}
                    onBlur={() => setTimeout(() => setSuggestionsOpen(false), 150)}
                    placeholder="Escribe un nombre… (Enter o coma para agregar nuevo)"
                    autoComplete="off"
                  />
                  {suggestionsOpen && (
                    <div style={{
                      position: 'absolute', top: '100%', left: 0, right: 0, zIndex: 1055,
                      border: '1px solid #dee2e6', borderRadius: '0.25rem', background: '#fff',
                      maxHeight: 180, overflowY: 'auto', boxShadow: '0 2px 8px rgba(0,0,0,0.15)'
                    }}>
                      {coauthorSuggestions.map(s => (
                        <div
                          key={s.id}
                          onMouseDown={() => addCoauthor({ id: s.id, name: s.name, type: s.type })}
                          style={{ padding: '0.4rem 0.75rem', cursor: 'pointer' }}
                          className="suggestion-item"
                          onMouseEnter={e => e.currentTarget.style.background = '#f0f4ff'}
                          onMouseLeave={e => e.currentTarget.style.background = ''}
                        >
                          <i className={`bi ${s.type === 'user' ? 'bi-person-plus text-success' : 'bi-person text-muted'} me-2`}></i>
                          {s.name}
                          {s.type === 'user' && <small className="ms-1 text-muted">(usuario)</small>}
                        </div>
                      ))}
                    </div>
                  )}
                </div>
                <small className="text-muted">Selecciona de la lista o escribe un nombre nuevo y presiona Enter. <span className="text-success">Verde</span> = usuario del sistema (se registrará como autor automáticamente).</small>
              </FormGroup>
            )}
          </Form>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" outline onClick={() => setModal(false)} disabled={formLoading}>
            Cancelar
          </Button>
          <Button color="primary" onClick={handleSubmit} disabled={formLoading}>
            {formLoading
              ? <><Spinner size="sm" className="me-1" /> Guardando…</>
              : editing ? 'Guardar cambios' : 'Crear publicación'
            }
          </Button>
        </ModalFooter>
      </Modal>

      {/* ── Modal confirmar borrado ── */}
      <Modal isOpen={deleteModal} toggle={() => setDeleteModal(false)} size="sm">
        <ModalHeader toggle={() => setDeleteModal(false)}>Eliminar publicación</ModalHeader>
        <ModalBody>
          {deleteError && <Alert color="danger">{deleteError}</Alert>}
          <p>
            ¿Seguro que quieres eliminar <strong>"{toDelete?.title}"</strong>?
            Esta acción no se puede deshacer.
          </p>
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" outline onClick={() => setDeleteModal(false)} disabled={deleteLoading}>
            Cancelar
          </Button>
          <Button color="danger" onClick={handleDelete} disabled={deleteLoading}>
            {deleteLoading
              ? <><Spinner size="sm" className="me-1" /> Eliminando…</>
              : 'Eliminar'
            }
          </Button>
        </ModalFooter>
      </Modal>
    </>
  );
}
