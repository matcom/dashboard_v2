using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Proyectos.Commands.UpdateProyectoEmpresarial;

public record UpdateProyectoEmpresarialCommand : IRequest<Result>
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
    public DateOnly FechaInicio { get; init; }
    public DateOnly? FechaCierre { get; init; }
    public string EstadoDeEjecucion { get; init; } = default!;
    public string CodigoProyecto { get; init; } = default!;
    public string EntidadEjecutoraPrincipal { get; init; } = default!;
    public string? EntidadEjecutoraParticipante { get; init; }
    public string? ContribucionSectoresEstrategicos { get; init; }
    public string? ContribucionEjesEstrategicos { get; init; }
    public bool TributaDesarrolloLocal { get; init; }
    public string Empresa { get; init; } = default!;
}

public class UpdateProyectoEmpresarialCommandHandler
    : IRequestHandler<UpdateProyectoEmpresarialCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateProyectoEmpresarialCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result> Handle(
        UpdateProyectoEmpresarialCommand request, CancellationToken cancellationToken)
    {
        var proyecto = await _context.Proyectos.OfType<ProyectoEmpresarial>()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (proyecto is null)
            return Result.Failure(["Proyecto no encontrado."]);

        if (!await _context.Clasificaciones.AnyAsync(c => c.Id == request.ClasificacionId, cancellationToken))
            return Result.Failure(["La clasificación indicada no existe."]);

        ProyectoHelper.SetBase(proyecto, request.Titulo, request.Jefe, request.CorreoJefe,
            request.NumeroMiembros, request.CantidadMiembrosUH, request.CantidadEstudiantes,
            request.CantidadEstudiantesContratados, request.TributaFormacionDoctoral,
            request.ClasificacionId);
        ProyectoHelper.SetEjecucion(proyecto, request.FechaInicio, request.FechaCierre,
            request.EstadoDeEjecucion, request.CodigoProyecto, request.EntidadEjecutoraPrincipal,
            request.EntidadEjecutoraParticipante, request.ContribucionSectoresEstrategicos,
            request.ContribucionEjesEstrategicos, request.TributaDesarrolloLocal);

        proyecto.Empresa = request.Empresa?.Trim() ?? string.Empty;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
