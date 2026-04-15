using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Queries.GetProyectoEnRevision;

public record GetProyectoEnRevisionQuery(string Id) : IRequest<ProyectoEnRevisionDto?>;

public class GetProyectoEnRevisionQueryHandler : IRequestHandler<GetProyectoEnRevisionQuery, ProyectoEnRevisionDto?>
{
    private readonly IApplicationDbContext _context;
    public GetProyectoEnRevisionQueryHandler(IApplicationDbContext context) => _context = context;

    public async Task<ProyectoEnRevisionDto?> Handle(GetProyectoEnRevisionQuery request, CancellationToken ct)
    {
        var p = await _context.Proyectos.OfType<ProyectoEnRevision>()
            .Include(x => x.Clasificacion)
            .FirstOrDefaultAsync(x => x.Id == request.Id, ct);

        if (p is null) return null;

        return new ProyectoEnRevisionDto
        {
            Id = p.Id, Titulo = p.Titulo, Jefe = p.Jefe, CorreoJefe = p.CorreoJefe,
            NumeroMiembros = p.NumeroMiembros, CantidadMiembrosUH = p.CantidadMiembrosUH,
            CantidadEstudiantes = p.CantidadEstudiantes,
            CantidadEstudiantesContratados = p.CantidadEstudiantesContratados,
            TributaFormacionDoctoral = p.TributaFormacionDoctoral,
            ClasificacionId = p.ClasificacionId, ClasificacionNombre = p.Clasificacion.Nombre,
            Situacion = p.Situacion, Tipo = p.Tipo,
        };
    }
}
