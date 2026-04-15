using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Commands.CreateProyectoEnRevision;

public record CreateProyectoEnRevisionCommand : IRequest<(Result Result, string? Id)>
{
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

public class CreateProyectoEnRevisionCommandHandler
    : IRequestHandler<CreateProyectoEnRevisionCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateProyectoEnRevisionCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<(Result Result, string? Id)> Handle(
        CreateProyectoEnRevisionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return (Result.Failure(["El título es obligatorio."]), null);

        if (!await _context.Clasificaciones.AnyAsync(c => c.Id == request.ClasificacionId, cancellationToken))
            return (Result.Failure(["La clasificación indicada no existe."]), null);

        var proyecto = new ProyectoEnRevision();
        ProyectoHelper.SetBase(proyecto, request.Titulo, request.Jefe, request.CorreoJefe,
            request.NumeroMiembros, request.CantidadMiembrosUH, request.CantidadEstudiantes,
            request.CantidadEstudiantesContratados, request.TributaFormacionDoctoral,
            request.ClasificacionId);

        proyecto.Situacion = request.Situacion?.Trim() ?? string.Empty;
        proyecto.Tipo = request.Tipo?.Trim() ?? string.Empty;

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), proyecto.Id);
    }
}
