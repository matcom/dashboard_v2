using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Documents.Reports;

/// <summary>
/// Reporte del anexo de grupos científicos estudiantiles.
/// URL generada: GET /api/Documents/anexo-grupos-estudiantiles
/// Plantilla:    Infrastructure/Templates/AnexoGruposEstudiantiles.xlsx
///
/// Columnas rellenadas automáticamente:
/// Nombre del grupo, Área temática UH, Línea de investigación.
///
/// Columnas que permanecen vacías para completado manual:
/// Total de integrantes, Áreas de la UH de sus miembros, No estudiantes 1ro-2do,
/// No estudiantes 3ro-4to y Proyectos de investigación y/o extensión vinculados.
/// Estas columnas no pueden derivarse hoy del modelo de dominio actual.
/// </summary>
public sealed class AnexoGruposEstudiantilesReport : IDocumentReport
{
    private readonly IApplicationDbContext _context;

    /// <summary>
    /// Inicializa el reporte con acceso al contexto de aplicación.
    /// </summary>
    /// <param name="context">Contexto usado para consultar los grupos estudiantiles.</param>
    public AnexoGruposEstudiantilesReport(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Identificador público del reporte consumido por el endpoint genérico de documentos.
    /// </summary>
    public string ReportName => "anexo-grupos-estudiantiles";

    /// <summary>
    /// Nombre base de la plantilla embebida sin extensión.
    /// </summary>
    public string TemplateName => "AnexoGruposEstudiantiles";

    /// <summary>
    /// Proyecta los grupos estudiantiles a una estructura plana compatible con
    /// ClosedXML.Report. La clave del diccionario debe coincidir exactamente con
    /// el nombre del rango definido en la plantilla.
    /// </summary>
    /// <param name="ct">Token para cancelar la consulta.</param>
    /// <returns>Variables que se inyectarán en la plantilla Excel.</returns>
    public async Task<IReadOnlyDictionary<string, object>> GatherVariablesAsync(CancellationToken ct)
    {
        var rows = await _context.GruposEstudiantiles
            .OrderBy(g => g.Nombre)
            .Select(g => new AnexoGruposEstudiantilesRowDto
            {
                Nombre = g.Nombre,
                AreaTematica = g.Area.Nombre,
                LineasDeInvestigacion = string.Join(", ", g.LineasDeInvestigacion
                    .OrderBy(l => l.Nombre)
                    .Select(l => l.Nombre)),
            })
            .ToListAsync(ct);

        return new Dictionary<string, object>
        {
            ["GruposEstudiantiles"] = rows,
        };
    }
}

/// <summary>
/// Proyección plana usada por ClosedXML.Report para resolver las expresiones
/// {{item.Xxx}} declaradas en la plantilla del anexo estudiantil.
/// </summary>
public sealed record AnexoGruposEstudiantilesRowDto
{
    /// <summary>
    /// Nombre del grupo científico estudiantil.
    /// </summary>
    public string Nombre { get; init; } = string.Empty;

    /// <summary>
    /// Área temática asociada al grupo.
    /// </summary>
    public string AreaTematica { get; init; } = string.Empty;

    /// <summary>
    /// Listado textual de líneas de investigación asociadas al grupo.
    /// </summary>
    public string LineasDeInvestigacion { get; init; } = string.Empty;
}
