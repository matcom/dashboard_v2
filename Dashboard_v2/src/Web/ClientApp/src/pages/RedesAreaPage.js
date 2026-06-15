import React, { useState, useEffect, useCallback } from 'react';
import { Card, CardBody, CardHeader, Spinner, Alert } from 'reactstrap';
import FilterableDataTable from '../components/FilterableDataTable';

const TIPOS_RED = [
  { value: 0, label: 'Universitaria' },
  { value: 1, label: 'Nacional' },
  { value: 2, label: 'Internacional' },
];

async function apiFetch(url) {
  const res = await fetch(url, { credentials: 'include' });
  const data = await res.json().catch(() => null);
  if (!res.ok) throw new Error((data?.errors ?? ['Error desconocido.']).join(' '));
  return data;
}

export default function RedesAreaPage() {
  const [items, setItems]     = useState([]);
  const [loading, setLoading] = useState(true);
  const [error, setError]     = useState('');

  const load = useCallback(async () => {
    setLoading(true); setError('');
    try { setItems(await apiFetch('/api/Redes')); }
    catch (e) { setError(e.message); }
    finally { setLoading(false); }
  }, []);

  useEffect(() => { load(); }, [load]);

  if (loading) return <div className="d-flex justify-content-center mt-5"><Spinner color="primary" /></div>;

  return (
    <>
      <h2 className="mb-4">Redes del Área</h2>
      {error && <Alert color="danger">{error}</Alert>}

      <Card>
        <CardHeader>
          <strong>Redes</strong>
          <small className="text-muted ms-2">({items.length})</small>
        </CardHeader>
        <CardBody className="p-0">
          <FilterableDataTable
            filterConfig={{
              search: { fields: ['nombre', 'countryName'], placeholder: 'Buscar red...' },
              filters: [
                {
                  key: 'tipo',
                  label: 'Tipo',
                  options: TIPOS_RED.map(t => ({ value: t.value, label: t.label })),
                  match: (item, val) => item.tipo === Number(val),
                },
              ],
            }}
            columns={[
              { key: 'nombre',             label: 'Nombre',   sortable: true },
              { key: 'tipo',               label: 'Tipo',     render: v => TIPOS_RED.find(t => t.value === v)?.label ?? v },
              { key: 'countryName',        label: 'País' },
              { key: 'cantidadProfesores', label: 'Profesores' },
            ]}
            data={items}
            keyExtractor={i => i.id}
            emptyMessage="No hay redes en el área."
          />
        </CardBody>
      </Card>
    </>
  );
}
