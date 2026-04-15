using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Commands.UpdateProyectoEnRevision;

public record UpdateProyectoEnRevisionCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Titulo { get; init; } = default!;
    public string Jefe { get; init; } = default!;
    public string CorreoJefe { get; init; } = default!;
    public int NumeroMiembros { get; init; }
    public int CantidadMiembrosUH { get; init; }
    public int CantidadEstudiantes { get; init; }
    public int CantidadEstudiantesContratados { get; init; }
    public bool TributaFormacionDoctoral { get; init; }
    public string ClasificacionId { get; init; } = default!;
    public string Situacion { get; init; } = default!;
    public string Tipo { get; init; } = default!;
}

public class UpdateProyectoEnRevisionCommandHandler
    : IRequestHandler<UpdateProyectoEnRevisionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateProyectoEnRevisionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result> Handle(
        UpdateProyectoEnRevisionCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await _context.Proyectos.OfType<ProyectoEnRevision>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (proyecto is null)
            return Result.Failure(["Proyecto no encontrado."]);

        if (!await _context.Clasificaciones.AnyAsync(c => c.Id == request.ClasificacionId, cancellationToken))
            return Result.Failure(["La clasificación indicada no existe."]);

        ProyectoHelper.SetBase(proyecto, request.Titulo, request.Jefe, request.CorreoJefe,
            request.NumeroMiembros, request.CantidadMiembrosUH, request.CantidadEstudiantes,
            request.CantidadEstudiantesContratados, request.TributaFormacionDoctoral,
            request.ClasificacionId);

        proyecto.Situacion = request.Situacion?.Trim() ?? string.Empty;
        proyecto.Tipo = request.Tipo?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
