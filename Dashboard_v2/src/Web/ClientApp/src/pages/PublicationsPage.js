import React, { useState, useEffect, useCallback } from 'react';
import {
  Card, CardBody, CardHeader,
  Nav, NavItem, NavLink,
  TabContent, TabPane,
  Table, Button, Badge,
  Spinner, Alert,
  Modal, ModalHeader, ModalBody, ModalFooter,
  Form, FormGroup, Label, Input,
} from 'reactstrap';
import { useAuth } from '../contexts/AuthContext';
import CoauthorPicker from '../components/CoauthorPicker';

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
  publicationType: 0,
  proyectoId: '',
  // Indexadas (tipos 1-4)
  index: '',
  // Revista (tipo 0)
  dataBase: '',
  group: '',
  cuartil: '',
  // Flag para indicar si al guardar se debe intentar resolver la base/grupo desde CrossRef
  resolveDatabaseFromCrossRef: false,
};

export default function PublicationsPage() {
  const { user } = useAuth();
  const [publications, setPublications] = useState([]);
  const [types, setTypes] = useState([]);
  const [proyectos, setProyectos] = useState([]);
  const [proyectosError, setProyectosError] = useState(false);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState('');

  // Modal de crear / editar
  const [modal, setModal] = useState(false);
  const [editing, setEditing] = useState(null); // null = crear, objeto = editar
  const [form, setForm] = useState(EMPTY_FORM);
  const [formError, setFormError] = useState('');
  const [formLoading, setFormLoading] = useState(false);
  // Duplicates modal state
  const [duplicatesModal, setDuplicatesModal] = useState(false);
  const [duplicatesCandidates, setDuplicatesCandidates] = useState([]);
  const [duplicatesError, setDuplicatesError] = useState('');
  const [duplicatesLoading, setDuplicatesLoading] = useState(false);

  // CrossRef search
  const [crossrefModal, setCrossrefModal] = useState(false);
  const [crossrefCandidates, setCrossrefCandidates] = useState([]);
  const [crossrefError, setCrossrefError] = useState('');
  const [crossrefLoading, setCrossrefLoading] = useState(false);
  // Resolver ahora state
  const [resolveLoading, setResolveLoading] = useState(false);
  const [resolveError, setResolveError] = useState('');

  // Detalles modal para ver una publicación candidata
  const [detailsModal, setDetailsModal] = useState(false);
  const [detailsPublication, setDetailsPublication] = useState(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [detailsError, setDetailsError] = useState('');
  const [detailsReturnToCrossRef, setDetailsReturnToCrossRef] = useState(false);

  // Modal de confirmación de borrado
  const [deleteModal, setDeleteModal] = useState(false);
  const [toDelete, setToDelete] = useState(null);
  const [deleteLoading, setDeleteLoading] = useState(false);
  const [deleteError, setDeleteError] = useState('');
  // Pestañas
  const [activeType, setActiveType] = useState(0);
  const [activeGroup, setActiveGroup] = useState(1);
  // Tag-picker de coautores
  const [coauthorTags, setCoauthorTags] = useState([]); // [{id?, name}]
  const loadData = useCallback(async () => {
    setLoading(true);
    setError('');
    try {
      const [pubs, pubTypes, cats] = await Promise.all([
        apiFetch('/api/Publications'),
        apiFetch('/api/Publications/types'),
        apiFetch('/api/Proyectos/catalogo').catch(() => null),
      ]);
      setPublications(pubs);
      setTypes(pubTypes);
      if (cats === null) {
        setProyectosError(true);
        setProyectos([]);
      } else {
        setProyectosError(false);
        setProyectos(cats);
      }
    } catch (e) {
      setError(e.message);
    } finally {
      setLoading(false);
    }
  }, []);

  useEffect(() => { loadData(); }, [loadData]);

  // ── helpers de formulario ──────────────────────────────────────────────────

  function openCreate(typeVal, groupVal) {
    setEditing(null);
    setForm({
      ...EMPTY_FORM,
      publicationType: typeVal ?? types[0]?.value ?? 0,
      group: groupVal != null ? String(groupVal) : '',
    });
    setFormError('');
    setCoauthorTags([]);
    setModal(true);
  }

  /**
   * Convierte un autor de la API al modelo consumido por el picker de tarjetas.
   * Si el autor está vinculado a un usuario, conserva ese snapshot para renderizar la tarjeta rica.
   */
  function mapAuthorToPickerEntry(author) {
    return {
      id: author.id,
      name: author.name,
      type: 'author',
      linkedUser: author.linkedUser ?? null,
    };
  }

  function openEdit(pub) {
    setEditing(pub);
    setForm({
      title: pub.title,
      publicationData: pub.publicationData,
      urlDoi: pub.urlDoi ?? '',
      publicationType: pub.publicationType,
      proyectoId: pub.proyectoId ?? '',
      // Indexadas
      index: pub.indexedPublication?.index ?? '',
      // Revista
      dataBase: pub.journalPublication?.dataBase ?? '',
      group: pub.journalPublication?.group ?? '',
      cuartil: pub.journalPublication?.cuartil ?? '',
    });
    setFormError('');
    // Pre-cargar coautores (todos excepto el usuario actual)
    const initialTags = (pub.authors ?? [])
      .filter(a => a.userId !== user?.id)
      .map(mapAuthorToPickerEntry);
    setCoauthorTags(initialTags);
    setModal(true);
  }

  function handleFormChange(e) {
    const { name, type, value, checked } = e.target;
    setForm(f => ({ ...f, [name]: type === 'checkbox' ? checked : value }));
  }

  function canSearchCrossRef() {
    return Boolean(form.title.trim() || form.urlDoi.trim());
  }

  async function handleSubmit() {
    if (!form.title.trim() || form.publicationType === '') {
      setFormError('El título y el tipo de publicación son obligatorios.');
      return;
    }
    setFormLoading(true);
    setFormError('');
    try {
      if (editing) {
        // PUT — actualizar: primero comprobar duplicados (excluir la propia publicación)
        try {
          setDuplicatesLoading(true);
          setDuplicatesError('');
          const candidates = await apiFetch(`/api/Publications/duplicates?title=${encodeURIComponent(form.title)}&doi=${encodeURIComponent(form.urlDoi || '')}&url=&excludeId=${encodeURIComponent(editing.id)}`);
          if (candidates && candidates.length > 0) {
            setDuplicatesCandidates(candidates);
            setDuplicatesModal(true);
            return; // wait for user's decision in modal
          }

          // no duplicates -> actualizar inmediatamente
          await performUpdate();
        } finally {
          setDuplicatesLoading(false);
        }
      } else {
        // POST — crear: primero comprobar duplicados
        try {
          setDuplicatesLoading(true);
          setDuplicatesError('');
          const candidates = await apiFetch(`/api/Publications/duplicates?title=${encodeURIComponent(form.title)}&doi=${encodeURIComponent(form.urlDoi || '')}&url=`);
          if (candidates && candidates.length > 0) {
            setDuplicatesCandidates(candidates);
            setDuplicatesModal(true);
            return; // wait for user's decision in modal
          }

          // no duplicates -> crear inmediatamente
          await performCreate();
        } finally {
          setDuplicatesLoading(false);
        }
      }
      setModal(false);
      loadData();
    } catch (e) {
      setFormError(e.message);
    } finally {
      setFormLoading(false);
    }
  }

  async function performCreate() {
    await apiFetch('/api/Publications', {
      method: 'POST',
      body: JSON.stringify({
        title: form.title,
        publicationData: form.publicationData,
        publicationType: parseInt(form.publicationType, 10),
        urlDoi: form.urlDoi || null,
        proyectoId: form.proyectoId || null,
        additionalAuthorIds: coauthorTags.filter(t => t.type === 'author').map(t => t.id),
        additionalAuthorNames: coauthorTags.filter(t => t.type === 'new').map(t => t.name),
        additionalUserIds: coauthorTags.filter(t => t.type === 'user').map(t => t.id),
        // Especialización
        index: parseInt(form.publicationType, 10) !== 0 ? form.index || null : null,
        dataBase: parseInt(form.publicationType, 10) === 0 ? form.dataBase || null : null,
        group: parseInt(form.publicationType, 10) === 0 ? parseInt(form.group, 10) || null : null,
        cuartil: parseInt(form.publicationType, 10) === 0 && parseInt(form.group, 10) === 1 ? form.cuartil || null : null,
        // Control explícito: si true, el servidor intentará resolver DB/Grupo desde CrossRef
        resolveDatabaseFromCrossRef: !!form.resolveDatabaseFromCrossRef,
      }),
    });
    setModal(false);
    loadData();
  }

  async function performUpdate() {
    if (!editing) return;
    await apiFetch(`/api/Publications/${editing.id}`, {
      method: 'PUT',
      body: JSON.stringify({
        title: form.title,
        publicationData: form.publicationData,
        publicationType: parseInt(form.publicationType, 10),
        urlDoi: form.urlDoi || null,
        proyectoId: form.proyectoId || null,
        additionalAuthorIds: coauthorTags.filter(t => t.type === 'author').map(t => t.id),
        additionalAuthorNames: coauthorTags.filter(t => t.type === 'new').map(t => t.name),
        additionalUserIds: coauthorTags.filter(t => t.type === 'user').map(t => t.id),
        // Especialización
        index: parseInt(form.publicationType, 10) !== 0 ? form.index || null : null,
        dataBase: parseInt(form.publicationType, 10) === 0 ? form.dataBase || null : null,
        group: parseInt(form.publicationType, 10) === 0 ? parseInt(form.group, 10) || null : null,
        cuartil: parseInt(form.publicationType, 10) === 0 && parseInt(form.group, 10) === 1 ? form.cuartil || null : null,
        // Control explícito: si true, el servidor intentará resolver DB/Grupo desde CrossRef
        resolveDatabaseFromCrossRef: !!form.resolveDatabaseFromCrossRef,
      }),
    });
    setModal(false);
    setEditing(null);
    loadData();
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
    } catch (e) {
      setDeleteError(e.message);
    } finally {
      setDeleteLoading(false);
    }
  }

  // ── render ─────────────────────────────────────────────────────────────────

  const pubsByType = (typeVal) => publications.filter(p => p.publicationType === typeVal);
  const journalByGroup = (g) => publications.filter(p => p.publicationType === 0 && p.journalPublication?.group === g);

  const authorsList = (authors) => authors.map((a, i) => (
    <span key={a.id}>
      {i > 0 && <span className="text-muted me-1">,</span>}
      {a.name}
      {a.userId && (
        <i className="bi bi-person-check ms-1 text-success" title="Usuario registrado"></i>
      )}
    </span>
  ));

  const actionBtns = (pub) => (
    <>
      <Button color="outline-primary" size="sm" className="me-1" onClick={() => openEdit(pub)}>
        <i className="bi bi-pencil"></i>
      </Button>
      <Button color="outline-danger" size="sm" onClick={() => openDelete(pub)}>
        <i className="bi bi-trash"></i>
      </Button>
    </>
  );

  function renderMatchLabel(type, score) {
    if (!type) return <span className="text-muted">—</span>;
    const map = {
      doi: 'DOI (coincidencia exacta)',
      url: 'URL (coincidencia)',
      title: 'Título (coincidencia exacta)'
    };
    const label = map[type] ?? type;
    return (
      <div>
        <strong>{label}</strong>
        {typeof score === 'number' && score < 1 && (
          <div className="text-muted" style={{ fontSize: '0.85em' }}>similitud: {(score * 100).toFixed(0)}%</div>
        )}
      </div>
    );
  }

  async function viewDetails(id) {
    setDetailsReturnToCrossRef(false);
    setDetailsError('');
    setDetailsLoading(true);
    try {
      const pub = await apiFetch(`/api/Publications/public/${id}`);
      setDetailsPublication(pub);
      setDetailsModal(true);
    } catch (e) {
      setDetailsError(e.message);
    } finally {
      setDetailsLoading(false);
    }
  }

  async function searchCrossRef() {
    if (!canSearchCrossRef()) {
      setCrossrefError('Escribe al menos un título o un DOI antes de buscar en CrossRef.');
      return;
    }

    setCrossrefError('');
    setCrossrefLoading(true);
    try {
      const res = await apiFetch(`/api/Publications/crossref?doi=${encodeURIComponent(form.urlDoi || '')}&title=${encodeURIComponent(form.title || '')}`);
      setCrossrefCandidates(res || []);
      setCrossrefModal(true);
    } catch (e) {
      setCrossrefError(e.message);
    } finally {
      setCrossrefLoading(false);
    }
  }

  async function resolveDatabaseNow() {
    setResolveError('');
    if (!canSearchCrossRef()) {
      setResolveError('Escribe al menos un título o un DOI antes de resolver.');
      return;
    }
    setResolveLoading(true);
    try {
      const res = await apiFetch(`/api/Publications/resolve-database?doi=${encodeURIComponent(form.urlDoi || '')}&title=${encodeURIComponent(form.title || '')}`);
      if (res) {
        setForm(f => ({
          ...f,
          dataBase: res.databaseName ?? f.dataBase,
          group: res.group != null ? String(res.group) : f.group,
          cuartil: res.cuartil ?? f.cuartil,
        }));
      }
    } catch (e) {
      setResolveError(e.message);
    } finally {
      setResolveLoading(false);
    }
  }

  function importFromCrossRef(candidate) {
    if (!candidate) return;
    setForm(f => ({
      ...f,
      title: candidate.title || f.title,
      urlDoi: candidate.doi ? `https://doi.org/${candidate.doi}` : (candidate.url || f.urlDoi),
      publicationData: candidate.publicationData || f.publicationData,
      publicationType: candidate.suggestedPublicationType ?? f.publicationType,
    }));
    setCrossrefModal(false);
  }

  function buildCrossRefPreview(candidate) {
    const publicationType = candidate?.suggestedPublicationType ?? null;
    const publicationTypeName = publicationType == null
      ? null
      : (types.find(t => t.value === publicationType)?.name ?? null);

    return {
      title: candidate?.title ?? 'Sin título',
      publicationData: candidate?.publicationData ?? '',
      urlDoi: candidate?.doi ? `https://doi.org/${candidate.doi}` : (candidate?.url ?? null),
      authors: (candidate?.authors ?? []).map((name, index) => ({
        id: `crossref-author-${index}`,
        name,
        linkedUser: null,
      })),
      publicationTypeName,
      crossRef: {
        doi: candidate?.doi ?? null,
        url: candidate?.url ?? null,
        containerTitle: candidate?.containerTitle ?? null,
        publisher: candidate?.publisher ?? null,
        published: candidate?.published ?? null,
        type: candidate?.type ?? null,
        volume: candidate?.volume ?? null,
        issue: candidate?.issue ?? null,
        page: candidate?.page ?? null,
        issns: candidate?.issns ?? [],
        isbns: candidate?.isbns ?? [],
      },
    };
  }

  function previewCrossRefCandidate(candidate) {
    setDetailsReturnToCrossRef(true);
    setDetailsError('');
    setDetailsLoading(false);
    setDetailsPublication(buildCrossRefPreview(candidate));
    setCrossrefModal(false);
    setDetailsModal(true);
  }

  function closeDetailsModal() {
    setDetailsModal(false);
    setDetailsError('');
    setDetailsLoading(false);
    setDetailsPublication(null);

    if (detailsReturnToCrossRef) {
      setCrossrefModal(true);
      setDetailsReturnToCrossRef(false);
    }
  }

  function renderPublicationDetails(pub) {
    if (!pub) return null;

    const isCrossRefPreview = Boolean(pub.crossRef);

    return (
      <div>
        <h5>{pub.title}</h5>
        {pub.publicationTypeName && (
          <p><strong>Tipo:</strong> {pub.publicationTypeName}</p>
        )}
        {!isCrossRefPreview && pub.publicationData && (
          <p style={{ whiteSpace: 'pre-line' }}>{pub.publicationData}</p>
        )}
        <p>
          <strong>URL / DOI:</strong>{' '}
          {pub.urlDoi
            ? <a href={pub.urlDoi} target="_blank" rel="noopener noreferrer">{pub.urlDoi}</a>
            : <span className="text-muted">—</span>}
        </p>
        <p><strong>Autores:</strong></p>
        <ul>
          {(pub.authors || []).length > 0
            ? (pub.authors || []).map(a => (
              <li key={a.id}>
                {a.name}
                {a.linkedUser ? ' (usuario del sistema)' : ''}
              </li>
            ))
            : <li className="text-muted">—</li>}
        </ul>
        {pub.journalPublication && (
          <p><strong>Base de datos:</strong> {pub.journalPublication.dataBase} (Grupo {pub.journalPublication.group}) {pub.journalPublication.cuartil && <>- {pub.journalPublication.cuartil}</>}</p>
        )}
        {pub.indexedPublication && (
          <p><strong>Indexación:</strong> {pub.indexedPublication.index}</p>
        )}
        {pub.proyectoTitulo && (
          <p><strong>Proyecto vinculado:</strong> {pub.proyectoTitulo}</p>
        )}
        {pub.crossRef && (
          <>
            <p><strong>Fuente CrossRef:</strong> {pub.crossRef.containerTitle ?? <span className="text-muted">—</span>}</p>
            <p><strong>Publisher:</strong> {pub.crossRef.publisher ?? <span className="text-muted">—</span>}</p>
            <p><strong>Fecha publicada:</strong> {pub.crossRef.published ?? <span className="text-muted">—</span>}</p>
            <p><strong>Tipo CrossRef:</strong> {pub.crossRef.type ?? <span className="text-muted">—</span>}</p>
            <p><strong>Volumen:</strong> {pub.crossRef.volume ?? <span className="text-muted">—</span>}</p>
            <p><strong>Número:</strong> {pub.crossRef.issue ?? <span className="text-muted">—</span>}</p>
            <p><strong>Páginas:</strong> {pub.crossRef.page ?? <span className="text-muted">—</span>}</p>
            <p><strong>ISSN:</strong> {pub.crossRef.issns?.length ? pub.crossRef.issns.join(', ') : <span className="text-muted">—</span>}</p>
            <p><strong>ISBN:</strong> {pub.crossRef.isbns?.length ? pub.crossRef.isbns.join(', ') : <span className="text-muted">—</span>}</p>
          </>
        )}
      </div>
    );
  }

  const urlCell = (urlDoi) => urlDoi
    ? <a href={urlDoi} target="_blank" rel="noopener noreferrer"
         className="text-truncate d-block" style={{ maxWidth: 170 }} title={urlDoi}>{urlDoi}</a>
    : <span className="text-muted">—</span>;

  return (
    <>
      <Card className="shadow-sm">
        <CardHeader><strong>Mis publicaciones</strong></CardHeader>
        <CardBody>
          {loading && <div className="text-center py-4"><Spinner /></div>}
          {error && <Alert color="danger">{error}</Alert>}

          {!loading && !error && (
            <>
              {/* ── Pestañas de tipo ── */}
              <Nav tabs>
                {types.map(t => (
                  <NavItem key={t.value}>
                    <NavLink
                      href="#"
                      active={activeType === t.value}
                      onClick={e => { e.preventDefault(); setActiveType(t.value); }}
                    >
                      {t.name}{' '}
                      <Badge color="secondary" pill style={{ fontSize: '0.7em' }}>
                        {pubsByType(t.value).length}
                      </Badge>
                    </NavLink>
                  </NavItem>
                ))}
              </Nav>

              <TabContent activeTab={String(activeType)} className="border border-top-0 rounded-bottom">

                {/* ── Pestaña Journal (tipo 0) ── */}
                <TabPane tabId="0" className="p-3">
                  <div className="d-flex align-items-center mb-3">
                    <Nav tabs className="flex-grow-1">
                      {[1, 2, 3, 4].map(g => (
                        <NavItem key={g}>
                          <NavLink
                            href="#"
                            active={activeGroup === g}
                            onClick={e => { e.preventDefault(); setActiveGroup(g); }}
                          >
                            Grupo {g}{' '}
                            <Badge color="secondary" pill style={{ fontSize: '0.7em' }}>
                              {journalByGroup(g).length}
                            </Badge>
                          </NavLink>
                        </NavItem>
                      ))}
                    </Nav>
                    <Button color="primary" size="sm" className="ms-3 flex-shrink-0"
                      onClick={() => openCreate(0, activeGroup)} disabled={loading}>
                      <i className="bi bi-plus-lg me-1"></i> Nueva publicación
                    </Button>
                  </div>

                  <TabContent activeTab={String(activeGroup)}>
                    {[1, 2, 3, 4].map(g => {
                      const pubs = journalByGroup(g);
                      return (
                        <TabPane tabId={String(g)} key={g}>
                          {pubs.length === 0
                            ? <p className="text-muted text-center py-3">No hay publicaciones en el Grupo {g}.</p>
                            : (
                              <Table hover responsive size="sm" className="mt-2 mb-0">
                                <thead>
                                  <tr>
                                    <th>Título</th>
                                    <th>Datos de pub.</th>
                                    <th>Base de datos</th>
                                    {g === 1 && <th>Cuartil</th>}
                                    <th>URL / DOI</th>
                                    <th>Autores</th>
                                    <th style={{ width: 90 }}></th>
                                  </tr>
                                </thead>
                                <tbody>
                                  {pubs.map(pub => (
                                    <tr key={pub.id}>
                                      <td>{pub.title}</td>
                                      <td>{pub.publicationData}</td>
                                      <td>{pub.journalPublication?.dataBase}</td>
                                      {g === 1 && (
                                        <td>
                                          {pub.journalPublication?.cuartil != null
                                            ? <Badge color="info" pill className="text-dark">{pub.journalPublication.cuartil}</Badge>
                                            : <span className="text-muted">—</span>}
                                        </td>
                                      )}
                                      <td style={{ maxWidth: 180 }}>{urlCell(pub.urlDoi)}</td>
                                      <td>{authorsList(pub.authors)}</td>
                                      <td className="text-end">{actionBtns(pub)}</td>
                                    </tr>
                                  ))}
                                </tbody>
                              </Table>
                            )
                          }
                        </TabPane>
                      );
                    })}
                  </TabContent>
                </TabPane>

                {/* ── Pestañas tipos indexados (1–4) ── */}
                {[1, 2, 3, 4].map(typeVal => {
                  const pubs = pubsByType(typeVal);
                  return (
                    <TabPane tabId={String(typeVal)} key={typeVal} className="p-3">
                      <div className="d-flex justify-content-end mb-2">
                        <Button color="primary" size="sm"
                          onClick={() => openCreate(typeVal, null)} disabled={loading}>
                          <i className="bi bi-plus-lg me-1"></i> Nueva publicación
                        </Button>
                      </div>
                      {pubs.length === 0
                        ? <p className="text-muted text-center py-3">No hay publicaciones de este tipo.</p>
                        : (
                          <Table hover responsive size="sm" className="mb-0">
                            <thead>
                              <tr>
                                <th>Título</th>
                                <th>Datos de pub.</th>
                                <th>Indexación</th>
                                <th>URL / DOI</th>
                                <th>Autores</th>
                                <th style={{ width: 90 }}></th>
                              </tr>
                            </thead>
                            <tbody>
                              {pubs.map(pub => (
                                <tr key={pub.id}>
                                  <td>{pub.title}</td>
                                  <td>{pub.publicationData}</td>
                                  <td>{pub.indexedPublication?.index ?? <span className="text-muted">—</span>}</td>
                                  <td style={{ maxWidth: 180 }}>{urlCell(pub.urlDoi)}</td>
                                  <td>{authorsList(pub.authors)}</td>
                                  <td className="text-end">{actionBtns(pub)}</td>
                                </tr>
                              ))}
                            </tbody>
                          </Table>
                        )
                      }
                    </TabPane>
                  );
                })}

              </TabContent>
            </>
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
              <Label for="publicationType">
                Tipo <span className="text-danger">*</span>
              </Label>
              <Input
                type="select"
                id="publicationType"
                name="publicationType"
                value={form.publicationType}
                onChange={handleFormChange}
              >
                {types.map(t => (
                  <option key={t.value} value={t.value}>{t.name}</option>
                ))}
              </Input>
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
              <div className="mt-2">
                <Button type="button" color="outline-secondary" size="sm" onClick={searchCrossRef} disabled={crossrefLoading}>
                  {crossrefLoading ? <Spinner size="sm" className="me-1" /> : null} Buscar en CrossRef
                </Button>
                <Button type="button" color="outline-primary" size="sm" className="ms-2" onClick={resolveDatabaseNow} disabled={resolveLoading}>
                  {resolveLoading ? <Spinner size="sm" className="me-1" /> : null} Resolver ahora
                </Button>
                <div className="text-muted small mt-1">
                  La búsqueda usa el título o el DOI actual y solo rellena el formulario para que puedas revisar y ajustar antes de guardar.
                </div>
                {crossrefError && <div className="text-danger small mt-1">{crossrefError}</div>}
                {resolveError && <div className="text-danger small mt-1">{resolveError}</div>}
                <FormGroup check className="mt-2">
                  <Label check>
                    <Input
                      type="checkbox"
                      id="resolveDatabaseFromCrossRef"
                      name="resolveDatabaseFromCrossRef"
                      checked={!!form.resolveDatabaseFromCrossRef}
                      onChange={handleFormChange}
                    />{' '}
                    Activar búsqueda de base de datos y cuartil desde CrossRef al guardar
                  </Label>
                </FormGroup>
              </div>
            </FormGroup>
            {proyectosError ? (
              <FormGroup>
                <Label>Proyecto derivado <small className="text-muted">(opcional)</small></Label>
                <Input disabled value="" />
                <small className="text-danger">No se pudieron cargar los proyectos.</small>
              </FormGroup>
            ) : (
              <FormGroup>
                <Label for="proyectoId">Proyecto derivado <small className="text-muted">(opcional)</small></Label>
                <Input
                  type="select"
                  id="proyectoId"
                  name="proyectoId"
                  value={form.proyectoId}
                  onChange={handleFormChange}
                >
                  <option value="">— Sin proyecto asociado —</option>
                  {proyectos.map(p => (
                    <option key={p.id} value={p.id}>{p.titulo}</option>
                  ))}
                </Input>
                {proyectos.length === 0 && (
                  <small className="text-muted">No hay proyectos disponibles.</small>
                )}
              </FormGroup>
            )}

            {/* ── Campos dinámicos por tipo ── */}
            {parseInt(form.publicationType, 10) === 0 ? (
              <>
                <FormGroup>
                  <Label for="dataBase">Base de datos <span className="text-danger">*</span></Label>
                  <Input
                    id="dataBase"
                    name="dataBase"
                    value={form.dataBase}
                    onChange={handleFormChange}
                    placeholder="Ej. Scopus, Web of Science..."
                  />
                </FormGroup>
                <FormGroup>
                  <Label for="group">Grupo (1–4) <span className="text-danger">*</span></Label>
                  <Input
                    type="select"
                    id="group"
                    name="group"
                    value={form.group}
                    onChange={handleFormChange}
                  >
                    <option value="">Selecciona grupo...</option>
                    {[1, 2, 3, 4].map(g => (
                      <option key={g} value={g}>Grupo {g}</option>
                    ))}
                  </Input>
                </FormGroup>
                {parseInt(form.group, 10) === 1 && (
                  <FormGroup>
                    <Label for="cuartil">Cuartil Scimago <span className="text-danger">*</span></Label>
                    <Input
                      type="select"
                      id="cuartil"
                      name="cuartil"
                      value={form.cuartil}
                      onChange={handleFormChange}
                    >
                      <option value="">Selecciona cuartil...</option>
                      {[1, 2, 3, 4].map(q => (
                        <option key={q} value={`Q${q}`}>Q{q}</option>
                      ))}
                    </Input>
                  </FormGroup>
                )}
              </>
            ) : (
              <FormGroup>
                <Label for="index">Indexación <span className="text-danger">*</span></Label>
                <Input
                  id="index"
                  name="index"
                  value={form.index}
                  onChange={handleFormChange}
                  placeholder="Ej. Scopus, SciELO, Latindex..."
                />
              </FormGroup>
            )}

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
                <CoauthorPicker
                  value={coauthorTags}
                  onChange={setCoauthorTags}
                  placeholder="Buscar coautor o escribir nombre libre..."
                  helpText="Selecciona una tarjeta del resultado o escribe un nombre nuevo y presiona Enter. Si el autor ya está vinculado a un usuario del sistema, verás su tarjeta institucional completa."
                />
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

      {/* ── Modal candidatos duplicados ── */}
      <Modal isOpen={duplicatesModal} toggle={() => setDuplicatesModal(false)} size="lg">
        <ModalHeader toggle={() => setDuplicatesModal(false)}>Publicaciones similares encontradas</ModalHeader>
        <ModalBody>
          {duplicatesError && <Alert color="danger">{duplicatesError}</Alert>}
          {duplicatesLoading && <div className="text-center py-3"><Spinner /></div>}
          {!duplicatesLoading && duplicatesCandidates.length === 0 && (
            <p className="text-muted">No se encontraron coincidencias.</p>
          )}
          {!duplicatesLoading && duplicatesCandidates.length > 0 && (
            <Table hover responsive size="sm">
              <thead>
                <tr>
                  <th>Título</th>
                  <th>URL / DOI</th>
                  <th>Coincidencia</th>
                  <th style={{ width: 220 }}></th>
                </tr>
              </thead>
              <tbody>
                {duplicatesCandidates.map(c => (
                  <tr key={c.id}>
                    <td style={{ maxWidth: 420 }}>{c.title}</td>
                    <td style={{ maxWidth: 220 }}>{c.urlDoi ?? '—'}</td>
                    <td>{renderMatchLabel(c.matchType, c.score)}</td>
                    <td className="text-end" style={{ whiteSpace: 'nowrap' }}>
                      <Button type="button" color="primary" size="sm" className="me-2"
                        onClick={async () => {
                          try {
                            await apiFetch(`/api/Publications/${c.id}/coauthors`, { method: 'POST' });
                            setDuplicatesModal(false);
                            setModal(false);
                            loadData();
                          } catch (e) {
                            setDuplicatesError(e.message);
                          }
                        }}>
                        Sí — añadirme como coautor
                      </Button>
                      <Button type="button" color="outline-secondary" size="sm" onClick={() => viewDetails(c.id)}>Ver</Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" outline onClick={() => { setDuplicatesModal(false); }} disabled={duplicatesLoading}>
            Volver
          </Button>
          <Button type="button" color="success" onClick={async () => { try { if (editing) await performUpdate(); else await performCreate(); setDuplicatesModal(false); } catch (e) { setDuplicatesError(e.message); } }} disabled={duplicatesLoading}>
            {editing ? 'Actualizar publicación' : 'Crear nueva publicación'}
          </Button>
        </ModalFooter>
      </Modal>

      {/* ── Modal resultados CrossRef ── */}
      <Modal isOpen={crossrefModal} toggle={() => setCrossrefModal(false)} size="lg">
        <ModalHeader toggle={() => setCrossrefModal(false)}>Resultados CrossRef</ModalHeader>
        <ModalBody>
          {crossrefError && <Alert color="danger">{crossrefError}</Alert>}
          {crossrefLoading && <div className="text-center py-3"><Spinner /></div>}
          {!crossrefLoading && crossrefCandidates.length === 0 && (
            <p className="text-muted">No se encontraron resultados en CrossRef.</p>
          )}
          {!crossrefLoading && crossrefCandidates.length > 0 && (
            <Table hover responsive size="sm">
              <thead>
                <tr>
                  <th>Título</th>
                  <th>DOI / URL</th>
                  <th>Fuente</th>
                  <th style={{ width: 220 }}></th>
                </tr>
              </thead>
              <tbody>
                {crossrefCandidates.map((c, idx) => (
                  <tr key={c.doi ?? c.url ?? idx}>
                    <td style={{ maxWidth: 420 }}>{c.title}</td>
                    <td style={{ maxWidth: 220 }}>{c.doi ? `https://doi.org/${c.doi}` : (c.url ?? '—')}</td>
                    <td>{c.containerTitle ?? '—'}</td>
                    <td className="text-end">
                      <Button type="button" color="primary" size="sm" className="me-2" onClick={() => importFromCrossRef(c)}>
                        Rellenar formulario
                      </Button>
                      <Button type="button" color="outline-secondary" size="sm" onClick={() => previewCrossRefCandidate(c)}>
                        Previsualizar
                      </Button>
                    </td>
                  </tr>
                ))}
              </tbody>
            </Table>
          )}
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" outline onClick={() => setCrossrefModal(false)}>Cerrar</Button>
        </ModalFooter>
      </Modal>

      {/* ── Modal ver detalles de publicación ── */}
      <Modal isOpen={detailsModal} toggle={closeDetailsModal} size="lg">
        <ModalHeader toggle={closeDetailsModal}>Detalles de la publicación</ModalHeader>
        <ModalBody>
          {detailsError && <Alert color="danger">{detailsError}</Alert>}
          {detailsLoading && <div className="text-center py-3"><Spinner /></div>}
          {detailsPublication && renderPublicationDetails(detailsPublication)}
        </ModalBody>
        <ModalFooter>
          <Button color="secondary" onClick={closeDetailsModal}>Cerrar</Button>
        </ModalFooter>
      </Modal>
    </>
  );
}
