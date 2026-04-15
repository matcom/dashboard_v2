using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Queries.GetProyectoColabInternacional;

public record GetProyectoColabInternacionalQuery(string Id) : IRequest<ProyectoColabInternacionalDto?>;

public class GetProyectoColabInternacionalQueryHandler : IRequestHandler<GetProyectoColabInternacionalQuery, ProyectoColabInternacionalDto?>
{
    private readonly IApplicationDbContext _context;
    public GetProyectoColabInternacionalQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProyectoColabInternacionalDto?> Handle(GetProyectoColabInternacionalQuery request, CancellationToken ct)
    {
        var p = await _context.Proyectos.OfType<ProyectoColabInternacional>()
            .Include(x => x.Clasificacion)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (p is null) return null;

        return new ProyectoColabInternacionalDto
        {
            Id = p.Id, Titulo = p.Titulo, Jefe = p.Jefe, CorreoJefe = p.CorreoJefe,
            NumeroMiembros = p.NumeroMiembros, CantidadMiembrosUH = p.CantidadMiembrosUH,
            CantidadEstudiantes = p.CantidadEstudiantes,
            CantidadEstudiantesContratados = p.CantidadEstudiantesContratados,
            TributaFormacionDoctoral = p.TributaFormacionDoctoral,
            ClasificacionId = p.ClasificacionId, ClasificacionNombre = p.Clasificacion.Nombre,
            FechaInicio = p.FechaInicio, FechaCierre = p.FechaCierre,
            EstadoDeEjecucion = p.EstadoDeEjecucion, CodigoProyecto = p.CodigoProyecto,
            EntidadEjecutoraPrincipal = p.EntidadEjecutoraPrincipal,
            EntidadEjecutoraParticipante = p.EntidadEjecutoraParticipante,
            ContribucionSectoresEstrategicos = p.ContribucionSectoresEstrategicos,
            ContribucionEjesEstrategicos = p.ContribucionEjesEstrategicos,
            TributaDesarrolloLocal = p.TributaDesarrolloLocal,
            FuenteFinanciacion = p.FuenteFinanciacion, TerminosReferencia = p.TerminosReferencia,
        };
    }
}
