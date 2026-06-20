using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using RolesEnum = Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.Documents.Reports;

/// <summary>
/// Reporte: Anexo 1 — Resumen anual por área (multi-hoja).
/// URL generada: GET /api/Documents/anexo-resumen
/// Plantilla:    Infrastructure/Templates/AnexoResumen.xlsx
///
/// Hojas con variables escalares ({{VarName}}):
///   Redes                — conteo de redes por tipo y profesores participantes
///   Patentes y Registros — Tabla 19: patentes / registros / normas
///   Nuevos Productos     — Tabla 18: productos / tecnologías / servicios
///   Ponencias en Eventos — ponencias en eventos nacionales e internacionales
///   Celebración          — eventos organizados y coauspiciados
///   Proyectos DL         — PDL totales y por estado
///   Publicaciones Resumen— Tabla 7 (parcial): grupos G1/G2/G3 y divulgación
///   Publicaciones Índices— índices por profesor y por doctor
///
/// Hojas con lista (Named Range + {{item.Field}}):
///   Premios — lista de tipos de premio con su cantidad cumplida
///
/// Parámetros opcionales:
///   from — fecha inicio (YYYY-MM) para filtrar publicaciones
///   to   — fecha fin   (YYYY-MM) para filtrar publicaciones
/// </summary>
public sealed class Anexo1Report : IDocumentReport
{
    private const int InternacionalEventTypeId = 0;
    private const int NacionalEventTypeId = 1;

    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public Anexo1Report(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public string ReportName => "anexo-1";
    public string TemplateName => "Anexo1";

    public IReadOnlyCollection<string> AllowedRoles =>
        [nameof(RolesEnum.Superuser), nameof(RolesEnum.Vicedecano_de_investigacion)];

    public async Task<IReadOnlyDictionary<string, object>> GatherVariablesAsync(
        IReadOnlyDictionary<string, string>? parameters,
        CancellationToken ct)
    {
        var areaId = await _context.GetUserAreaIdAsync(_currentUser.Id, ct);

        var fromRaw = parameters is not null && parameters.TryGetValue("from", out var f) ? f : null;
        var toRaw   = parameters is not null && parameters.TryGetValue("to",   out var t) ? t : null;
        var from    = string.IsNullOrWhiteSpace(fromRaw) ? null : fromRaw[..Math.Min(fromRaw.Length, 7)];
        var to      = string.IsNullOrWhiteSpace(toRaw)   ? null : toRaw[..Math.Min(toRaw.Length, 7)];

        var redesVars    = await GatherRedesAsync(areaId, ct);
        var regVars      = await GatherRegistrosYPatentesAsync(areaId, ct);
        var premios      = await GatherPremiosAsync(areaId, ct);
        var eventosVars  = await GatherEventosAsync(areaId, ct);
        var pdlVars      = await GatherProyectosDLAsync(areaId, ct);
        var pubsVars     = await GatherPublicacionesAsync(areaId, from, to, ct);
        int numProfesores = await CountProfesoresAsync(areaId, ct);

        double Ratio(int numerator) =>
            numProfesores > 0 ? Math.Round((double)numerator / numProfesores, 2) : 0.0;

        return new Dictionary<string, object>
        {
            // ── Redes ─────────────────────────────────────────────────────────
            ["RedesUniversitariasProfesores"] = redesVars.UniversitariasProfesores,
            ["RedesUniversitariasEstudiantes"] = 0,
            ["RedesNacionalesProfesores"]     = redesVars.NacionalesProfesores,
            ["RedesNacionalesEstudiantes"]    = 0,
            ["RedesInternacionalesProfesores"]= redesVars.InternacionalesProfesores,
            ["RedesInternacionalesEstudiantes"]= 0,
            ["RedesTotalProfesores"]          = redesVars.TotalProfesores,
            ["RedesTotalEstudiantes"]         = 0,

            // ── Patentes y Registros (Tabla 19) ───────────────────────────────
            ["PatentesCuba"]          = regVars.PatentesCuba,
            ["PatentesExtranjero"]    = regVars.PatentesExtranjero,
            ["PatentesTotal"]         = regVars.PatentesCuba + regVars.PatentesExtranjero,
            ["RegistrosNoInformaticos"]= regVars.RegistrosNoInformaticos,
            ["RegistrosInformaticos"] = regVars.RegistrosInformaticos,
            ["RegistrosTotal"]        = regVars.RegistrosNoInformaticos + regVars.RegistrosInformaticos,
            ["NormasNacionales"]      = regVars.NormasNacionales,
            ["NormasRamales"]         = regVars.NormasRamales,
            ["NormasEmpresariales"]   = regVars.NormasEmpresariales,
            ["NormasTotal"]           = regVars.NormasNacionales + regVars.NormasRamales + regVars.NormasEmpresariales,

            // ── Nuevos Productos (Tabla 18) ───────────────────────────────────
            ["NuevosProductos"]             = regVars.NuevosProductos,
            ["NuevasTecnologias"]           = regVars.NuevasTecnologias,
            ["NuevosServicios"]             = regVars.NuevosServicios,
            ["NuevosProductosTecServTotal"] = regVars.NuevosProductos + regVars.NuevasTecnologias + regVars.NuevosServicios,

            // ── Premios (Tabla 14 — list) ──────────────────────────────────────
            ["Premios"] = premios,

            // ── Eventos y Ponencias ───────────────────────────────────────────
            ["PonenciasEventosNacionalesReal"]      = eventosVars.PonenciasNacionales,
            ["PonenciasEventosInternacionalesReal"] = eventosVars.PonenciasInternacionales,
            ["PonenciasEventosTotalReal"]           = eventosVars.PonenciasNacionales + eventosVars.PonenciasInternacionales,
            ["PonenciasPorProfesor"]                = Ratio(eventosVars.PonenciasNacionales + eventosVars.PonenciasInternacionales),
            ["EventosOrganizadosReal"]              = eventosVars.EventosOrganizados,
            ["EventosCoauspiciadosReal"]            = eventosVars.EventosCoauspiciados,

            // ── Proyectos de Desarrollo Local ─────────────────────────────────
            ["PDLTotal"]          = pdlVars.Total,
            ["PDLTerminados"]     = pdlVars.Terminados,
            ["PDLEnEjecucion"]    = pdlVars.EnEjecucion,
            ["PDLAtrasados"]      = pdlVars.Atrasados,
            ["PDLCancelados"]     = pdlVars.Cancelados,
            ["PDLContribucionTotal"] = pdlVars.Total,

            // ── Publicaciones Resumen (Tabla 7 — parcial) ─────────────────────
            ["G1Count"]                   = pubsVars.G1,
            ["G2Count"]                   = pubsVars.G2,
            ["G3Count"]                   = pubsVars.G3,
            ["ArticulosDivulgacionCount"] = pubsVars.ArtDiv,

            // ── Publicaciones Índices ─────────────────────────────────────────
            ["IndicePublicacionesTotalProfesor"] = Ratio(pubsVars.G1 + pubsVars.G2 + pubsVars.G3 + pubsVars.G4 + pubsVars.ArtDiv),
            ["IndicePublicacionesTotalDoctor"]   = 0.0,
            ["IndicePublicacionesWosProfesor"]   = Ratio(pubsVars.G1 + pubsVars.G2),
            ["IndicePublicacionesWosDoctor"]     = 0.0,
            ["IndiceArticulosG2"]                = pubsVars.G2,
        };
    }

    // ─── Redes ──────────────────────────────────────────────────────────────────

    private async Task<RedesResumenVars> GatherRedesAsync(string? areaId, CancellationToken ct)
    {
        var reds = await _context.Reds
            .AsNoTracking()
            .Include(r => r.Coordinador)
            .Include(r => r.Participaciones).ThenInclude(p => p.Author).ThenInclude(a => a.User)
            .Where(r => areaId == null
                || (r.CoordinadorId != null && r.Coordinador!.AreaId == areaId)
                || r.Participaciones.Any(p => p.Author.UserId != null && p.Author.User!.AreaId == areaId))
            .ToListAsync(ct);

        return new RedesResumenVars
        {
            UniversitariasProfesores = reds.Where(r => r.Tipo == TipoRed.Universitaria).Sum(r => r.CantidadProfesores),
            NacionalesProfesores     = reds.Where(r => r.Tipo == TipoRed.Nacional).Sum(r => r.CantidadProfesores),
            InternacionalesProfesores= reds.Where(r => r.Tipo == TipoRed.Internacional).Sum(r => r.CantidadProfesores),
            TotalProfesores          = reds.Sum(r => r.CantidadProfesores),
        };
    }

    // ─── Patentes, Registros, Normas, Productos ─────────────────────────────────

    private async Task<RegistrosResumenVars> GatherRegistrosYPatentesAsync(string? areaId, CancellationToken ct)
    {
        var patentes = await _context.Patentes
            .AsNoTracking()
            .Where(p => areaId == null
                || p.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .Select(p => p.EsNacional)
            .ToListAsync(ct);

        var registros = await _context.Registros
            .AsNoTracking()
            .Where(r => areaId == null
                || r.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .Select(r => r.EsInformatico)
            .ToListAsync(ct);

        var normas = await _context.Normas
            .AsNoTracking()
            .Include(n => n.TipoNorma)
            .Where(n => n.TipoNormaId != null
                && (areaId == null
                    || n.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId)))
            .Select(n => n.TipoNorma!.Nombre)
            .ToListAsync(ct);

        var productos = await _context.ProductosComercializados
            .AsNoTracking()
            .Include(p => p.TipoProductoComercializado)
            .Where(p => areaId == null
                || p.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .Select(p => p.TipoProductoComercializado.Nombre)
            .ToListAsync(ct);

        return new RegistrosResumenVars
        {
            PatentesCuba             = patentes.Count(n => n),
            PatentesExtranjero       = patentes.Count(n => !n),
            RegistrosNoInformaticos  = registros.Count(r => !r),
            RegistrosInformaticos    = registros.Count(r => r),
            NormasNacionales         = normas.Count(n => n.Contains("nacional", StringComparison.OrdinalIgnoreCase)),
            NormasRamales            = normas.Count(n => n.Contains("ramal",    StringComparison.OrdinalIgnoreCase)),
            NormasEmpresariales      = normas.Count(n => n.Contains("empresa",  StringComparison.OrdinalIgnoreCase)),
            NuevosProductos          = productos.Count(p => p.Contains("product", StringComparison.OrdinalIgnoreCase)),
            NuevasTecnologias        = productos.Count(p => p.Contains("tecnolog", StringComparison.OrdinalIgnoreCase)),
            NuevosServicios          = productos.Count(p => p.Contains("servicio", StringComparison.OrdinalIgnoreCase)),
        };
    }

    // ─── Premios ────────────────────────────────────────────────────────────────

    private async Task<List<Anexo1PremioRowDto>> GatherPremiosAsync(string? areaId, CancellationToken ct)
    {
        var awardTypes = await _context.AwardTypes
            .AsNoTracking()
            .Include(at => at.Awards)
                .ThenInclude(a => a.UserAwardeds)
                    .ThenInclude(r => r.User)
            .OrderBy(at => at.Name)
            .ToListAsync(ct);

        return awardTypes
            .Select(at => new Anexo1PremioRowDto
            {
                TipoPremio = at.Name,
                Cantidad   = at.Awards
                    .SelectMany(a => a.UserAwardeds)
                    .Count(r => areaId == null || r.User?.AreaId == areaId),
            })
            .Where(row => row.Cantidad > 0)
            .ToList();
    }

    // ─── Eventos y Ponencias ────────────────────────────────────────────────────

    private async Task<EventosResumenVars> GatherEventosAsync(string? areaId, CancellationToken ct)
    {
        var events = await _context.Events
            .AsNoTracking()
            .Include(e => e.Organizadores).ThenInclude(o => o.User)
            .Include(e => e.Participaciones).ThenInclude(p => p.User)
            .Where(e => areaId == null
                || e.Organizadores.Any(o => o.User.AreaId == areaId)
                || e.Participaciones.Any(p => p.User.AreaId == areaId))
            .ToListAsync(ct);

        var presentations = await _context.Presentations
            .AsNoTracking()
            .Include(p => p.User)
            .Include(p => p.Event)
            .Where(p => areaId == null || p.User.AreaId == areaId)
            .ToListAsync(ct);

        var eventosOrganizados = string.IsNullOrWhiteSpace(areaId)
            ? events.Count
            : events.Count(e => e.Organizadores.Any(o => o.User?.AreaId == areaId));

        var eventosCoauspiciados = string.IsNullOrWhiteSpace(areaId)
            ? 0
            : events.Count(e => e.Organizadores.Any(o => o.User?.AreaId == areaId));

        return new EventosResumenVars
        {
            PonenciasNacionales      = presentations.Count(p => p.Event.EventTypeId == NacionalEventTypeId),
            PonenciasInternacionales = presentations.Count(p => p.Event.EventTypeId == InternacionalEventTypeId),
            EventosOrganizados       = eventosOrganizados,
            EventosCoauspiciados     = eventosCoauspiciados,
        };
    }

    // ─── Proyectos de Desarrollo Local ─────────────────────────────────────────

    private async Task<PdlResumenVars> GatherProyectosDLAsync(string? areaId, CancellationToken ct)
    {
        var pdls = await _context.Proyectos
            .OfType<ProyectoDesarrolloLocal>()
            .AsNoTracking()
            .Include(p => p.JefeUsuario)
            .Include(p => p.Participantes)
            .Include(p => p.EstadosDeEjecucion)
            .Where(p => areaId == null
                || p.JefeUsuario.AreaId == areaId
                || p.Participantes.Any(u => u.AreaId == areaId))
            .ToListAsync(ct);

        static bool HasEstado(ProyectoDesarrolloLocal p, string keyword) =>
            p.EstadosDeEjecucion.Any(e => e.Nombre.Contains(keyword, StringComparison.OrdinalIgnoreCase));

        return new PdlResumenVars
        {
            Total       = pdls.Count,
            Terminados  = pdls.Count(p => HasEstado(p, "terminad")),
            EnEjecucion = pdls.Count(p => HasEstado(p, "ejecuci")),
            Atrasados   = pdls.Count(p => HasEstado(p, "atras")),
            Cancelados  = pdls.Count(p => HasEstado(p, "cancel")),
        };
    }

    // ─── Publicaciones ───────────────────────────────────────────────────────────

    private async Task<PublicacionesResumenVars> GatherPublicacionesAsync(
        string? areaId, string? from, string? to, CancellationToken ct)
    {
        var publications = await _context.Publications
            .AsNoTracking()
            .Include(p => p.AuthorPublications).ThenInclude(ap => ap.Author).ThenInclude(a => a.User)
            .Include(p => p.JournalPublication)
            .Include(p => p.IndexedPublication)
            .Where(p => areaId == null
                || p.AuthorPublications.Any(ap => ap.Author.UserId != null && ap.Author.User!.AreaId == areaId))
            .ToListAsync(ct);

        var filtered = publications
            .Where(p =>
            {
                var d = p.PublishedDate;
                var dFrom = d.Length == 4 ? d + "-01" : d[..Math.Min(d.Length, 7)];
                var dTo   = d.Length == 4 ? d + "-12" : d[..Math.Min(d.Length, 7)];
                return (from == null || string.Compare(dFrom, from, StringComparison.Ordinal) >= 0)
                    && (to   == null || string.Compare(dTo,   to,   StringComparison.Ordinal) <= 0);
            })
            .ToList();

        var journal = filtered.Where(p => p.PublicationType == PublicationType.Diario && p.JournalPublication is not null).ToList();
        var indexed = filtered.Where(p => p.PublicationType != PublicationType.Diario && p.IndexedPublication is not null).ToList();

        return new PublicacionesResumenVars
        {
            G1     = journal.Count(p => p.JournalPublication!.Group == 1),
            G2     = journal.Count(p => p.JournalPublication!.Group == 2),
            G3     = journal.Count(p => p.JournalPublication!.Group == 3),
            G4     = journal.Count(p => p.JournalPublication!.Group == 4),
            ArtDiv = indexed.Count(p => p.PublicationType == PublicationType.Artículo_de_Divulgación),
        };
    }

    // ─── Profesores en área ──────────────────────────────────────────────────────

    private async Task<int> CountProfesoresAsync(string? areaId, CancellationToken ct) =>
        await _context.UserRoles
            .Where(ur => ur.Role == RolesEnum.Profesor
                && (areaId == null || ur.User.AreaId == areaId))
            .CountAsync(ct);

    // ─── Value-object helpers ────────────────────────────────────────────────────

    private sealed record RedesResumenVars
    {
        public int UniversitariasProfesores  { get; init; }
        public int NacionalesProfesores      { get; init; }
        public int InternacionalesProfesores { get; init; }
        public int TotalProfesores           { get; init; }
    }

    private sealed record RegistrosResumenVars
    {
        public int PatentesCuba            { get; init; }
        public int PatentesExtranjero      { get; init; }
        public int RegistrosNoInformaticos { get; init; }
        public int RegistrosInformaticos   { get; init; }
        public int NormasNacionales        { get; init; }
        public int NormasRamales           { get; init; }
        public int NormasEmpresariales     { get; init; }
        public int NuevosProductos         { get; init; }
        public int NuevasTecnologias       { get; init; }
        public int NuevosServicios         { get; init; }
    }

    private sealed record EventosResumenVars
    {
        public int PonenciasNacionales      { get; init; }
        public int PonenciasInternacionales { get; init; }
        public int EventosOrganizados       { get; init; }
        public int EventosCoauspiciados     { get; init; }
    }

    private sealed record PdlResumenVars
    {
        public int Total       { get; init; }
        public int Terminados  { get; init; }
        public int EnEjecucion { get; init; }
        public int Atrasados   { get; init; }
        public int Cancelados  { get; init; }
    }

    private sealed record PublicacionesResumenVars
    {
        public int G1     { get; init; }
        public int G2     { get; init; }
        public int G3     { get; init; }
        public int G4     { get; init; }
        public int ArtDiv { get; init; }
    }
}

/// <summary>Fila en la hoja "Premios" del Anexo Resumen.</summary>
public sealed record Anexo1PremioRowDto
{
    public string TipoPremio { get; init; } = string.Empty;
    public int    Cantidad   { get; init; }
}
