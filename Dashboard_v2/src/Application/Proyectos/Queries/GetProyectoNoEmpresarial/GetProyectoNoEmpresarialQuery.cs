using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Queries.GetProyectoNoEmpresarial;

public record GetProyectoNoEmpresarialQuery(string Id) : IRequest<ProyectoNoEmpresarialDto?>;

public class GetProyectoNoEmpresarialQueryHandler : IRequestHandler<GetProyectoNoEmpresarialQuery, ProyectoNoEmpresarialDto?>
{
    private readonly IApplicationDbContext _context;
    public GetProyectoNoEmpresarialQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProyectoNoEmpresarialDto?> Handle(GetProyectoNoEmpresarialQuery request, CancellationToken ct)
    {
        var p = await _context.Proyectos.OfType<ProyectoNoEmpresarial>()
            .Include(x => x.Clasificacion)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (p is null) return null;

        return new ProyectoNoEmpresarialDto
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
            EntidadNoEmpresarial = p.EntidadNoEmpresarial,
        };
    }
}
