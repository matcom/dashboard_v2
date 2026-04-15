using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Queries.GetProyectoApoyoPrograma;

public record GetProyectoApoyoProgramaQuery(string Id) : IRequest<ProyectoApoyoProgramaDto?>;

public class GetProyectoApoyoProgramaQueryHandler : IRequestHandler<GetProyectoApoyoProgramaQuery, ProyectoApoyoProgramaDto?>
{
    private readonly IApplicationDbContext _context;
    public GetProyectoApoyoProgramaQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProyectoApoyoProgramaDto?> Handle(GetProyectoApoyoProgramaQuery request, CancellationToken ct)
    {
        var p = await _context.Proyectos.OfType<ProyectoApoyoPrograma>()
            .Include(x => x.Clasificacion)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (p is null) return null;

        return new ProyectoApoyoProgramaDto
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
            NombrePrograma = p.NombrePrograma, TipoPAP = p.TipoPAP,
        };
    }
}
