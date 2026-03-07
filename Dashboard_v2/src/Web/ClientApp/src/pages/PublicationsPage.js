import React, { useState, useEffect, useCallback } from 'react';
import { apiFetch } from '../utils/apiFetch';
import { useAuth } from '../contexts/AuthContext';

/* ─────────────────────────────────────────────
   Helpers
───────────────────────────────────────────── */
const EMPTY_FORM = {
  title: '',
  authorRelation: '',
  publicationDate: '',
  publicationTypeId: '',
  isJournal: false,
  journalDatabase: '',
  journalGroupName: '',
  journalQuartile: '',
  isIndexedPublication: false,
  indexName: '',
};

function badgeType(pub) {
  if (pub.isJournal && pub.isIndexedPublication) return <span className="badge badge-purple">Revista + Indexada</span>;
  if (pub.isJournal)               return <span className="badge badge-blue">Revista</span>;
  if (pub.isIndexedPublication)    return <span className="badge badge-green">Indexada</span>;
  return null;
}

/* ─────────────────────────────────────────────
   PublicationsPage
───────────────────────────────────────────── */
export default function PublicationsPage() {
  const { hasPermission } = useAuth();
  const [publications, setPublications] = useState([]);
  const [types, setTypes]               = useState([]);
  const [pagination, setPagination]     = useState({ pageNumber: 1, totalPages: 1, totalCount: 0 });
  const [search, setSearch]             = useState('');
  const [loading, setLoading]           = useState(false);
  const [error, setError]               = useState(null);

  // Modal state
  const [modalOpen, setModalOpen]       = useState(false);
  const [editingId, setEditingId]       = useState(null);
  const [form, setForm]                 = useState(EMPTY_FORM);
  const [formErrors, setFormErrors]     = useState([]);
  const [saving, setSaving]             = useState(false);

  // Delete confirm
  const [deleteId, setDeleteId]         = useState(null);
  const [deleting, setDeleting]         = useState(false);

  /* ── Cargar datos ── */
  const loadPublications = useCallback(async (page = 1, term = search) => {
    setLoading(true);
    setError(null);
    try {
      const params = new URLSearchParams({ pageNumber: page, pageSize: 20 });
      if (term) params.set('search', term);
      const res = await apiFetch(`/api/Publications?${params}`);
      if (!res.ok) throw new Error('Error al cargar publicaciones');
      const data = await res.json();
      setPublications(data.items);
      setPagination({ pageNumber: data.pageNumber, totalPages: data.totalPages, totalCount: data.totalCount });
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, [search]);

  useEffect(() => {
    apiFetch('/api/Publications/types')
      .then(r => r.json())
      .then(setTypes);
  }, []);

  useEffect(() => {
    loadPublications(1, search);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  /* ── Búsqueda ── */
  const handleSearch = (e) => {
    e.preventDefault();
    loadPublications(1, search);
  };

  /* ── Abrir modal ── */
  const openCreate = () => {
    setEditingId(null);
    setForm(EMPTY_FORM);
    setFormErrors([]);
    setModalOpen(true);
  };

  const openEdit = async (id) => {
    setFormErrors([]);
    setEditingId(id);
    const res = await apiFetch(`/api/Publications/${id}`);
    const data = await res.json();
    setForm({
      title: data.title ?? '',
      authorRelation: data.authorRelation ?? '',
      publicationDate: data.publicationDate ?? '',
      publicationTypeId: data.publicationTypeId ?? '',
      isJournal: !!data.journal,
      journalDatabase: data.journal?.database ?? '',
      journalGroupName: data.journal?.groupName ?? '',
      journalQuartile: data.journal?.quartile ?? '',
      isIndexedPublication: !!data.indexedPublication,
      indexName: data.indexedPublication?.indexName ?? '',
    });
    setModalOpen(true);
  };

  /* ── Campo del formulario ── */
  const set = (field) => (e) => {
    const val = e.target.type === 'checkbox' ? e.target.checked : e.target.value;
    setForm(f => ({ ...f, [field]: val }));
  };

  /* ── Guardar ── */
  const handleSave = async () => {
    setSaving(true);
    setFormErrors([]);
    const body = {
      title: form.title,
      authorRelation: form.authorRelation || null,
      publicationDate: form.publicationDate || null,
      publicationTypeId: Number(form.publicationTypeId),
      isJournal: form.isJournal,
      journalDatabase: form.isJournal ? (form.journalDatabase || null) : null,
      journalGroupName: form.isJournal ? (form.journalGroupName || null) : null,
      journalQuartile: form.isJournal ? (form.journalQuartile || null) : null,
      isIndexedPublication: form.isIndexedPublication,
      indexName: form.isIndexedPublication ? (form.indexName || null) : null,
    };

    try {
      let res;
      if (editingId) {
        res = await apiFetch(`/api/Publications/${editingId}`, {
          method: 'PUT',
          body: JSON.stringify({ id: editingId, ...body }),
        });
      } else {
        res = await apiFetch('/api/Publications', {
          method: 'POST',
          body: JSON.stringify(body),
        });
      }

      if (!res.ok) {
        const err = await res.json();
        const msgs = err.errors
          ? Object.values(err.errors).flat()
          : [err.title ?? 'Error al guardar.'];
        setFormErrors(msgs);
        return;
      }

      setModalOpen(false);
      loadPublications(pagination.pageNumber, search);
    } finally {
      setSaving(false);
    }
  };

  /* ── Eliminar ── */
  const handleDelete = async () => {
    setDeleting(true);
    try {
      await apiFetch(`/api/Publications/${deleteId}`, { method: 'DELETE' });
      setDeleteId(null);
      loadPublications(pagination.pageNumber, search);
    } finally {
      setDeleting(false);
    }
  };

  /* ─────────────────────────────────────────────
     Render
  ───────────────────────────────────────────── */
  return (
    <div className="page-container">
      {/* Header */}
      <div className="page-header">
        <div>
          <h2 className="page-title">Publicaciones</h2>
          <p className="page-subtitle">Gestión de publicaciones científicas y académicas</p>
        </div>
        {hasPermission('publications.create') && (
          <button className="btn btn-primary" onClick={openCreate}>
            <i className="bi bi-plus-lg me-1"></i> Nueva publicación
          </button>
        )}
      </div>

      {/* Search */}
      <form className="search-bar" onSubmit={handleSearch}>
        <input
          className="search-bar__input"
          type="text"
          placeholder="Buscar por título o autor…"
          value={search}
          onChange={e => setSearch(e.target.value)}
        />
        <button className="btn btn-secondary" type="submit">
          <i className="bi bi-search"></i>
        </button>
      </form>

      {/* Error */}
      {error && <div className="alert alert-danger">{error}</div>}

      {/* Tabla */}
      <div className="table-card">
        {loading ? (
          <div className="table-placeholder">Cargando…</div>
        ) : publications.length === 0 ? (
          <div className="table-placeholder">
            <i className="bi bi-journals fs-2 d-block mb-2"></i>
            No hay publicaciones registradas.
          </div>
        ) : (
          <table className="data-table">
            <thead>
              <tr>
                <th>Título</th>
                <th>Tipo</th>
                <th>Autor / Afiliación</th>
                <th>Fecha</th>
                <th>Especialización</th>
                <th className="col-actions">Acciones</th>
              </tr>
            </thead>
            <tbody>
              {publications.map(pub => (
                <tr key={pub.id}>
                  <td className="col-title">{pub.title}</td>
                  <td><span className="badge badge-gray">{pub.publicationTypeName}</span></td>
                  <td className="col-author">{pub.authorRelation ?? '—'}</td>
                  <td>{pub.publicationDate ?? '—'}</td>
                  <td>{badgeType(pub)}</td>
                  <td className="col-actions">
                    {pub.canEdit && (
                      <button
                        className="btn-icon btn-icon--edit"
                        title="Editar"
                        onClick={() => openEdit(pub.id)}
                      >
                        <i className="bi bi-pencil"></i>
                      </button>
                    )}
                    {pub.canDelete && (
                      <button
                        className="btn-icon btn-icon--delete"
                        title="Eliminar"
                        onClick={() => setDeleteId(pub.id)}
                      >
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

      {/* Paginación */}
      {pagination.totalPages > 1 && (
        <div className="pagination">
          <button
            className="btn btn-secondary btn-sm"
            disabled={pagination.pageNumber <= 1}
            onClick={() => loadPublications(pagination.pageNumber - 1, search)}
          >
            <i className="bi bi-chevron-left"></i>
          </button>
          <span className="pagination__info">
            Página {pagination.pageNumber} de {pagination.totalPages} ({pagination.totalCount} registros)
          </span>
          <button
            className="btn btn-secondary btn-sm"
            disabled={pagination.pageNumber >= pagination.totalPages}
            onClick={() => loadPublications(pagination.pageNumber + 1, search)}
          >
            <i className="bi bi-chevron-right"></i>
          </button>
        </div>
      )}

      {/* ── Modal Crear / Editar ── */}
      {modalOpen && (
        <div className="modal-overlay" onClick={() => setModalOpen(false)}>
          <div className="modal-dialog" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">
                {editingId ? 'Editar publicación' : 'Nueva publicación'}
              </h3>
              <button className="modal-close" onClick={() => setModalOpen(false)}>
                <i className="bi bi-x-lg"></i>
              </button>
            </div>

            <div className="modal-body">
              {formErrors.length > 0 && (
                <div className="alert alert-danger">
                  <ul className="mb-0 ps-3">
                    {formErrors.map((e, i) => <li key={i}>{e}</li>)}
                  </ul>
                </div>
              )}

              {/* Título */}
              <div className="form-group">
                <label className="form-label">Título <span className="required">*</span></label>
                <input className="form-control" value={form.title} onChange={set('title')} placeholder="Título de la publicación" />
              </div>

              {/* Tipo */}
              <div className="form-group">
                <label className="form-label">Tipo de publicación <span className="required">*</span></label>
                <div className="select-wrapper">
                  <select className="form-control" value={form.publicationTypeId} onChange={set('publicationTypeId')}>
                    <option value="">— Seleccionar —</option>
                    {types.map(t => (
                      <option key={t.id} value={t.id}>{t.name}</option>
                    ))}
                  </select>
                </div>
              </div>

              {/* Autor */}
              <div className="form-group">
                <label className="form-label">Autor / Afiliación</label>
                <input className="form-control" value={form.authorRelation} onChange={set('authorRelation')} placeholder="Ej: Juan Pérez (UCLV)" />
              </div>

              {/* Fecha */}
              <div className="form-group">
                <label className="form-label">Fecha de publicación</label>
                <input className="form-control" type="date" value={form.publicationDate} onChange={set('publicationDate')} />
              </div>

              {/* ─── Especialización: Revista ─── */}
              <div className="form-section">
                <label className="form-check">
                  <input type="checkbox" checked={form.isJournal} onChange={set('isJournal')} />
                  <span>Es una Revista (Journal)</span>
                </label>
                {form.isJournal && (
                  <div className="form-subsection">
                    <div className="form-group">
                      <label className="form-label">Base de datos</label>
                      <input className="form-control" value={form.journalDatabase} onChange={set('journalDatabase')} placeholder="Ej: Scopus, Web of Science" />
                    </div>
                    <div className="form-row">
                      <div className="form-group flex-1">
                        <label className="form-label">Grupo</label>
                        <input className="form-control" value={form.journalGroupName} onChange={set('journalGroupName')} placeholder="Ej: STEM" />
                      </div>
                      <div className="form-group" style={{width: '140px'}}>
                        <label className="form-label">Cuartil</label>
                        <div className="select-wrapper">
                          <select className="form-control" value={form.journalQuartile} onChange={set('journalQuartile')}>
                            <option value="">—</option>
                            <option value="Q1">Q1</option>
                            <option value="Q2">Q2</option>
                            <option value="Q3">Q3</option>
                            <option value="Q4">Q4</option>
                          </select>
                        </div>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {/* ─── Especialización: Indexada ─── */}
              <div className="form-section">
                <label className="form-check">
                  <input type="checkbox" checked={form.isIndexedPublication} onChange={set('isIndexedPublication')} />
                  <span>Es una Publicación Indexada</span>
                </label>
                {form.isIndexedPublication && (
                  <div className="form-subsection">
                    <div className="form-group">
                      <label className="form-label">Índice / Base de datos</label>
                      <input className="form-control" value={form.indexName} onChange={set('indexName')} placeholder="Ej: Latindex, SciELO, PubMed" />
                    </div>
                  </div>
                )}
              </div>
            </div>

            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setModalOpen(false)} disabled={saving}>
                Cancelar
              </button>
              <button className="btn btn-primary" onClick={handleSave} disabled={saving}>
                {saving ? 'Guardando…' : (editingId ? 'Guardar cambios' : 'Registrar publicación')}
              </button>
            </div>
          </div>
        </div>
      )}

      {/* ── Modal Confirmar Eliminación ── */}
      {deleteId !== null && (
        <div className="modal-overlay" onClick={() => setDeleteId(null)}>
          <div className="modal-dialog modal-dialog--sm" onClick={e => e.stopPropagation()}>
            <div className="modal-header">
              <h3 className="modal-title">Confirmar eliminación</h3>
            </div>
            <div className="modal-body">
              <p>¿Estás seguro de que deseas eliminar esta publicación? Esta acción es irreversible y eliminará también todos los permisos asociados.</p>
            </div>
            <div className="modal-footer">
              <button className="btn btn-secondary" onClick={() => setDeleteId(null)} disabled={deleting}>
                Cancelar
              </button>
              <button className="btn btn-danger" onClick={handleDelete} disabled={deleting}>
                {deleting ? 'Eliminando…' : 'Eliminar'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
}
