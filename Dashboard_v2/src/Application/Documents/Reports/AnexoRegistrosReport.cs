using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RolesEnum = Dashboard_v2.Domain.Enums.Roles;

namespace Dashboard_v2.Application.Documents.Reports;

public sealed class AnexoRegistrosReport : IDocumentReport
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AnexoRegistrosReport(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public string ReportName => "anexo-registros";

    public string TemplateName => "AnexoRegistros";

    public IReadOnlyCollection<string> AllowedRoles =>
        [nameof(RolesEnum.Superuser), nameof(RolesEnum.Jefe_de_Grupo_de_investigacion), nameof(RolesEnum.Vicedecano_de_investigacion)];

    public async Task<IReadOnlyDictionary<string, object>> GatherVariablesAsync(IReadOnlyDictionary<string, string>? parameters, CancellationToken ct)
    {
        var areaId = await _context.GetUserAreaIdAsync(_currentUser.Id, ct);

        var patentes = await GatherPatentesAsync(areaId, ct);
        var registros = await GatherRegistrosAsync(areaId, ct);
        var normas = await GatherNormasAsync(areaId, ct);
        var tiposProducto = await GatherProductosAsync(areaId, ct);

        return new Dictionary<string, object>
        {
            ["Patentes"] = patentes,
            ["RegistrosInformaticos"] = registros.Where(r => r.EsInformatico).ToList(),
            ["RegistrosNoInformaticos"] = registros.Where(r => !r.EsInformatico).ToList(),
            ["Normas"] = normas,
            ["ProductosTipos"] = tiposProducto,
        };
    }

    private async Task<List<AnexoRegistrosPatenteRowDto>> GatherPatentesAsync(string? areaId, CancellationToken ct) =>
        await _context.Patentes
            .AsNoTracking()
            .Where(p => areaId == null
                || p.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .OrderBy(p => p.Titulo)
            .Select(p => new AnexoRegistrosPatenteRowDto
            {
                Titulo = p.Titulo,
                NumeroSolicitudConcesion = p.NumeroSolicitudConcesion,
                EsNacional = p.EsNacional,
            })
            .ToListAsync(ct);

    private async Task<List<AnexoRegistroRowDto>> GatherRegistrosAsync(string? areaId, CancellationToken ct) =>
        await _context.Registros
            .AsNoTracking()
            .Where(r => areaId == null
                || r.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .OrderBy(r => r.Titulo)
            .Select(r => new AnexoRegistroRowDto
            {
                Titulo = r.Titulo,
                InstitutionNombre = r.Institution.Nombre,
                NumeroCertificado = r.NumeroCertificado,
                CountryName = r.Country.Name,
                EsInformatico = r.EsInformatico,
            })
            .ToListAsync(ct);

    private async Task<List<AnexoNormaRowDto>> GatherNormasAsync(string? areaId, CancellationToken ct) =>
        await _context.Normas
            .AsNoTracking()
            .Where(n => areaId == null
                || n.Creadores.Any(c => c.Author.UserId != null && c.Author.User!.AreaId == areaId))
            .OrderBy(n => n.Titulo)
            .Select(n => new AnexoNormaRowDto
            {
                Titulo = n.Titulo,
                Tipo = n.TipoNorma != null ? n.TipoNorma.Nombre : string.Empty,
                InstitutionNombre = n.Institution.Nombre,
            })
            .ToListAsync(ct);

    private async Task<List<AnexoProductoTipoRowDto>> GatherProductosAsync(string? areaId, CancellationToken ct)
    {
        var tipos = await _context.TipoProductosComercializados
            .AsNoTracking()
            .Include(t => t.Productos)
                .ThenInclude(p => p.Institution)
            .Include(t => t.Productos)
                .ThenInclude(p => p.Creadores)
                    .ThenInclude(c => c.Author)
                        .ThenInclude(a => a.User)
            .OrderBy(t => t.Nombre)
            .ToListAsync(ct);

        return tipos
            .Select(t => new AnexoProductoTipoRowDto
            {
                TipoProductoComercializadoNombre = t.Nombre,
                Productos = t.Productos
                    .Where(p => areaId == null
                        || p.Creadores.Any(c => c.Author.UserId != null && c.Author.User?.AreaId == areaId))
                    .OrderBy(p => p.Titulo)
                    .Select(p => new AnexoProductoRowDto
                    {
                        Titulo = p.Titulo,
                        InstitutionNombre = p.Institution?.Nombre ?? string.Empty,
                    })
                    .ToList()
            })
            .Where(t => t.Productos.Count > 0)
            .ToList();
    }
}

public sealed record AnexoRegistrosPatenteRowDto
{
    public string Titulo { get; init; } = string.Empty;
    public string NumeroSolicitudConcesion { get; init; } = string.Empty;
    public bool EsNacional { get; init; }
}

public sealed record AnexoRegistroRowDto
{
    public string Titulo { get; init; } = string.Empty;
    public string InstitutionNombre { get; init; } = string.Empty;
    public string NumeroCertificado { get; init; } = string.Empty;
    public string CountryName { get; init; } = string.Empty;
    public bool EsInformatico { get; init; }
}

public sealed record AnexoNormaRowDto
{
    public string Titulo { get; init; } = string.Empty;
    public string Tipo { get; init; } = string.Empty;
    public string InstitutionNombre { get; init; } = string.Empty;
}

public sealed record AnexoProductoTipoRowDto
{
    public string TipoProductoComercializadoNombre { get; init; } = string.Empty;
    public IReadOnlyList<AnexoProductoRowDto> Productos { get; init; } = [];
}

public sealed record AnexoProductoRowDto
{
    public string Titulo { get; init; } = string.Empty;
    public string InstitutionNombre { get; init; } = string.Empty;
}
