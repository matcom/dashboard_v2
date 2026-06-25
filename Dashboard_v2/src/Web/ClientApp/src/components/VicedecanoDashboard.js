import React, { useState, useEffect, useMemo } from 'react';
import { Spinner } from 'reactstrap';
import {
  BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer,
  PieChart, Pie, Cell, Legend,
} from 'recharts';

// ── Paleta ────────────────────────────────────────────────────────────────────
const COLORS = ['#4f86c6', '#56b87e', '#e07b39', '#9c65c1', '#d4526e', '#3cb4c5', '#e8c53a', '#6c757d'];

// ── Primitivos de UI ──────────────────────────────────────────────────────────

function StatCard({ icon, label, value, color = 'stat-blue', link, sublabel }) {
  const inner = (
    <div className={`stat-card ${color}`}>
      <div className="stat-card__icon"><i className={`bi ${icon}`} /></div>
      <div className="stat-card__body">
        <span className="stat-card__value">{value != null ? value : '—'}</span>
        <span className="stat-card__label">{label}</span>
        {sublabel && <span className="stat-card__sublabel">{sublabel}</span>}
      </div>
    </div>
  );
  if (link) return <a href={link} style={{ textDecoration: 'none', display: 'block' }}>{inner}</a>;
  return inner;
}

function Section({ title, children, className = '' }) {
  return (
    <div className={`vd-section ${className}`}>
      <h5 className="vd-section__title">{title}</h5>
      {children}
    </div>
  );
}

function Empty() {
  return <p className="vd-empty">Sin datos</p>;
}

// ── Gráficos ──────────────────────────────────────────────────────────────────

function HBarChart({ data, dataKey = 'cantidad', nameKey = 'label', color = '#4f86c6', height = 220 }) {
  if (!data?.length) return <Empty />;
  return (
    <ResponsiveContainer width="100%" height={height}>
      <BarChart data={data} layout="vertical" margin={{ left: 8, right: 24, top: 4, bottom: 4 }}>
        <CartesianGrid strokeDasharray="3 3" horizontal={false} />
        <XAxis type="number" allowDecimals={false} tick={{ fontSize: 12 }} />
        <YAxis type="category" dataKey={nameKey} width={150} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Bar dataKey={dataKey} fill={color} radius={[0, 4, 4, 0]} />
      </BarChart>
    </ResponsiveContainer>
  );
}

function VBarChart({ data, dataKey = 'cantidad', nameKey = 'label', color = '#4f86c6', height = 220 }) {
  if (!data?.length) return <Empty />;
  return (
    <ResponsiveContainer width="100%" height={height}>
      <BarChart data={data} margin={{ left: 0, right: 8, top: 4, bottom: 4 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} />
        <XAxis dataKey={nameKey} tick={{ fontSize: 12 }} />
        <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Bar dataKey={dataKey} fill={color} radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  );
}

function MultiVBarChart({ data, bars, nameKey = 'label', height = 240 }) {
  if (!data?.length) return <Empty />;
  return (
    <ResponsiveContainer width="100%" height={height}>
      <BarChart data={data} margin={{ left: 0, right: 8, top: 4, bottom: 4 }}>
        <CartesianGrid strokeDasharray="3 3" vertical={false} />
        <XAxis dataKey={nameKey} tick={{ fontSize: 12 }} />
        <YAxis allowDecimals={false} tick={{ fontSize: 12 }} />
        <Tooltip />
        <Legend wrapperStyle={{ fontSize: 12 }} />
        {bars.map((b, i) => (
          <Bar key={b.key} dataKey={b.key} name={b.label} fill={COLORS[i % COLORS.length]} radius={[4, 4, 0, 0]} />
        ))}
      </BarChart>
    </ResponsiveContainer>
  );
}

function DonutChart({ data, nameKey = 'label', valueKey = 'cantidad', height = 240 }) {
  if (!data?.length) return <Empty />;
  return (
    <ResponsiveContainer width="100%" height={height}>
      <PieChart>
        <Pie
          data={data}
          dataKey={valueKey}
          nameKey={nameKey}
          cx="50%"
          cy="50%"
          innerRadius="38%"
          outerRadius="62%"
          paddingAngle={2}
          label={({ name, percent }) => `${name} ${(percent * 100).toFixed(0)}%`}
          labelLine={false}
        >
          {data.map((_, i) => <Cell key={i} fill={COLORS[i % COLORS.length]} />)}
        </Pie>
        <Tooltip formatter={(v, n) => [v, n]} />
        <Legend wrapperStyle={{ fontSize: 12 }} />
      </PieChart>
    </ResponsiveContainer>
  );
}

function MiniTable({ rows, col1 = 'Tipo', col2 = 'Cantidad' }) {
  if (!rows?.length) return <Empty />;
  return (
    <table className="vd-mini-table">
      <thead><tr><th>{col1}</th><th>{col2}</th></tr></thead>
      <tbody>
        {rows.map((r, i) => (
          <tr key={i}><td>{r.label}</td><td>{r.cantidad}</td></tr>
        ))}
      </tbody>
    </table>
  );
}

// ── Tabla paginada ─────────────────────────────────────────────────────────────

const PAGE_SIZE = 10;

function PaginatedTable({ rows, col1 = 'Nombre', col2 = 'Publicaciones' }) {
  const [page, setPage] = useState(0);
  const [search, setSearch] = useState('');

  const filtered = useMemo(() =>
    rows?.filter(r => r.label.toLowerCase().includes(search.toLowerCase())) ?? [],
    [rows, search]
  );

  const pages = Math.ceil(filtered.length / PAGE_SIZE);
  const slice = filtered.slice(page * PAGE_SIZE, (page + 1) * PAGE_SIZE);

  const handleSearch = (e) => { setSearch(e.target.value); setPage(0); };

  if (!rows?.length) return <Empty />;
  return (
    <div>
      <input
        className="form-control form-control-sm mb-2"
        placeholder="Buscar..."
        value={search}
        onChange={handleSearch}
      />
      <table className="vd-mini-table">
        <thead><tr><th>{col1}</th><th>{col2}</th></tr></thead>
        <tbody>
          {slice.map((r, i) => (
            <tr key={i}><td>{r.label}</td><td>{r.cantidad}</td></tr>
          ))}
        </tbody>
      </table>
      {pages > 1 && (
        <div className="vd-pagination">
          <button
            className="btn btn-sm btn-outline-secondary"
            disabled={page === 0}
            onClick={() => setPage(p => p - 1)}
          >‹</button>
          <span>{page + 1} / {pages}</span>
          <button
            className="btn btn-sm btn-outline-secondary"
            disabled={page >= pages - 1}
            onClick={() => setPage(p => p + 1)}
          >›</button>
        </div>
      )}
    </div>
  );
}

// ── Tabla de redes ─────────────────────────────────────────────────────────────

const TIPO_BADGE = {
  Universitaria: 'badge bg-info text-dark',
  Nacional:      'badge bg-success',
  Internacional: 'badge bg-primary',
};

function RedesTable({ redes }) {
  if (!redes?.length) return <Empty />;
  return (
    <table className="vd-mini-table">
      <thead><tr><th>Red</th><th>Tipo</th></tr></thead>
      <tbody>
        {redes.map((r, i) => (
          <tr key={i}>
            <td>{r.nombre}</td>
            <td><span className={TIPO_BADGE[r.tipo] ?? 'badge bg-secondary'}>{r.tipo}</span></td>
          </tr>
        ))}
      </tbody>
    </table>
  );
}

// ── Componente principal ───────────────────────────────────────────────────────

export default function VicedecanoDashboard() {
  const [data, setData] = useState(null);
  const [error, setError] = useState(false);

  useEffect(() => {
    fetch('/api/Dashboard/vicedecano', { credentials: 'include' })
      .then(r => r.ok ? r.json() : Promise.reject())
      .then(setData)
      .catch(() => setError(true));
  }, []);

  if (error) return <p className="text-danger">No se pudo cargar el dashboard del área.</p>;
  if (!data)  return <div className="d-flex justify-content-center py-5"><Spinner color="primary" /></div>;

  const p = data.plantilla ?? {};

  return (
    <div className="vd-dashboard">

      {/* ══ 1. Totales generales ══════════════════════════════════════════════ */}
      <div className="stats-grid">
        <StatCard icon="bi-people-fill"              label="Personal"       value={data.totalUsuarios}      color="stat-teal"   />
        <StatCard icon="bi-trophy-fill"              label="Premios"        value={data.totalPremios}       color="stat-purple" link="/premios-area" />
        <StatCard icon="bi-file-earmark-text-fill"  label="Publicaciones"  value={data.totalPublicaciones} color="stat-blue"   link="/publicaciones-area" />
        <StatCard icon="bi-mic-fill"                label="Ponencias"      value={data.totalPonencias}     color="stat-green"  link="/events" />
        <StatCard icon="bi-calendar-event-fill"     label="Eventos"        value={data.totalEventos}       color="stat-orange" link="/events" />
        <StatCard icon="bi-folder-fill"             label="Proyectos"      value={data.totalProyectos}     color="stat-teal"   link="/proyectos-area" />
        <StatCard icon="bi-share-fill"              label="Redes"          value={data.totalRedes}         color="stat-blue"   link="/redes-area" />
        <StatCard icon="bi-people-fill"             label="Grupos Inv."    value={data.totalGrupos}        color="stat-green"  link="/grupos-investigacion-area" />
        <StatCard icon="bi-lightbulb-fill"          label="Patentes"       value={data.totalPatentes}      color="stat-purple" link="/patentes-area" />
        <StatCard icon="bi-shield-check-fill"       label="Registros"      value={data.totalRegistros}     color="stat-teal"   link="/registros-area" />
        <StatCard icon="bi-file-ruled-fill"         label="Normas"         value={data.totalNormas}        color="stat-orange" link="/normas-area" />
        <StatCard icon="bi-box-seam-fill"           label="Productos"      value={data.totalProductos}     color="stat-blue"   link="/productos-area" />
      </div>

      {/* ══ 2. Plantilla / Personal ══════════════════════════════════════════ */}
      <h4 className="vd-group-title">Plantilla</h4>

      {/* Totales de plantilla */}
      <div className="stats-grid stats-grid--sm">
        <StatCard icon="bi-mortarboard-fill" label="Docentes"       value={p.totalDocentes}       color="stat-blue" />
        <StatCard icon="bi-search"           label="Investigadores"  value={p.totalInvestigadores}  color="stat-green" />
      </div>

      <div className="vd-grid-3">
        <Section title="Categoría científica">
          <HBarChart data={p.porCategoriaCientifica} color="#9c65c1" height={200} />
        </Section>
        <Section title="Categoría docente">
          <HBarChart data={p.porCategoriaDocente} color="#4f86c6" height={200} />
        </Section>
        <Section title="Categoría de investigación">
          <HBarChart data={p.porCategoriaInvestigacion} color="#56b87e" height={200} />
        </Section>
      </div>

      {/* ══ 3. Publicaciones ═════════════════════════════════════════════════ */}
      <h4 className="vd-group-title">Publicaciones</h4>

      <div className="vd-grid-2">
        <Section title="Por grupo de indexación">
          <VBarChart data={data.publicacionesPorGrupo} color="#4f86c6" />
        </Section>
        <Section title="Por tipo de publicación">
          <DonutChart data={data.publicacionesPorTipo} />
        </Section>
      </div>

      <div className="vd-grid-1">
        <Section title="Por año de publicación">
          <VBarChart data={data.publicacionesPorAno} color="#3cb4c5" height={200} />
        </Section>
      </div>

      <div className="vd-grid-1">
        <Section title="Publicaciones por profesor">
          <PaginatedTable rows={data.publicacionesPorProfesor} col1="Profesor" col2="Publicaciones" />
        </Section>
      </div>

      {/* ══ 4. Proyectos ═════════════════════════════════════════════════════ */}
      <h4 className="vd-group-title">Proyectos</h4>

      <div className="vd-grid-2">
        <Section title="Por tipo de proyecto">
          <DonutChart data={data.proyectosPorTipo} />
        </Section>
        <Section title="Por estado de ejecución">
          <HBarChart data={data.proyectosPorEstado} color="#e07b39" />
        </Section>
      </div>

      {/* ══ 5. Premios ═══════════════════════════════════════════════════════ */}
      <h4 className="vd-group-title">Premios</h4>

      <div className="vd-grid-2">
        <Section title="Por tipo de premio">
          <HBarChart data={data.premiosPorTipo} color="#9c65c1" />
        </Section>
        <Section title="Por año de otorgamiento">
          <VBarChart data={data.premiosPorAno} color="#d4526e" />
        </Section>
      </div>

      {/* ══ 6. Eventos y Ponencias ════════════════════════════════════════════ */}
      <h4 className="vd-group-title">Eventos y Ponencias</h4>

      <div className="vd-grid-2">
        <Section title="Eventos por tipo">
          <HBarChart data={data.eventosPorTipo} color="#e07b39" />
        </Section>
        <Section title="Eventos por año">
          <VBarChart data={data.eventosPorAno} color="#e8c53a" />
        </Section>
      </div>

      <div className="vd-grid-1">
        <Section title="Ponencias por año">
          <VBarChart data={data.ponenciasPorAno} color="#56b87e" height={200} />
        </Section>
      </div>

      {/* ══ 7. Redes ═════════════════════════════════════════════════════════ */}
      <h4 className="vd-group-title">Redes Científicas</h4>

      <div className="vd-grid-2">
        <Section title="Distribución por tipo">
          <DonutChart data={data.redesPorTipo} />
        </Section>
        <Section title="Redes del área">
          <RedesTable redes={data.redesDelArea} />
        </Section>
      </div>

      {/* ══ 8. Propiedad Intelectual ══════════════════════════════════════════ */}
      <h4 className="vd-group-title">Propiedad Intelectual</h4>

      <div className="vd-grid-2">
        <Section title="Patentes por origen">
          <DonutChart data={data.patentesPorOrigen} />
        </Section>
        <Section title="Registros por tipo">
          <DonutChart data={data.registrosPorTipo} />
        </Section>
      </div>

      <div className="vd-grid-2">
        <Section title="Normas por tipo">
          <HBarChart data={data.normasPorTipo} color="#3cb4c5" />
        </Section>
        <Section title="Productos comercializados por tipo">
          <HBarChart data={data.productosPorTipo} color="#e07b39" />
        </Section>
      </div>

    </div>
  );
}
