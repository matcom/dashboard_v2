using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Proyectos.Commands.CreateProyectoApoyoPrograma;

public record CreateProyectoApoyoProgramaCommand : IRequest<(Result Result, string? Id)>
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
    public DateOnly FechaInicio { get; init; }
    public DateOnly? FechaCierre { get; init; }
    public string EstadoDeEjecucion { get; init; } = default!;
    public string CodigoProyecto { get; init; } = default!;
    public string EntidadEjecutoraPrincipal { get; init; } = default!;
    public string? EntidadEjecutoraParticipante { get; init; }
    public string? ContribucionSectoresEstrategicos { get; init; }
    public string? ContribucionEjesEstrategicos { get; init; }
    public bool TributaDesarrolloLocal { get; init; }
    public string NombrePrograma { get; init; } = default!;
    public TipoPAP TipoPAP { get; init; }
}

public class CreateProyectoApoyoProgramaCommandHandler
    : IRequestHandler<CreateProyectoApoyoProgramaCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateProyectoApoyoProgramaCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<(Result Result, string? Id)> Handle(
        CreateProyectoApoyoProgramaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Titulo))
            return (Result.Failure(["El título es obligatorio."]), null);

        if (!await _context.Clasificaciones.AnyAsync(c => c.Id == request.ClasificacionId, cancellationToken))
            return (Result.Failure(["La clasificación indicada no existe."]), null);

        var proyecto = new ProyectoApoyoPrograma();
        ProyectoHelper.SetBase(proyecto, request.Titulo, request.Jefe, request.CorreoJefe,
            request.NumeroMiembros, request.CantidadMiembrosUH, request.CantidadEstudiantes,
            request.CantidadEstudiantesContratados, request.TributaFormacionDoctoral,
            request.ClasificacionId);
        ProyectoHelper.SetEjecucion(proyecto, request.FechaInicio, request.FechaCierre,
            request.EstadoDeEjecucion, request.CodigoProyecto, request.EntidadEjecutoraPrincipal,
            request.EntidadEjecutoraParticipante, request.ContribucionSectoresEstrategicos,
            request.ContribucionEjesEstrategicos, request.TributaDesarrolloLocal);

        proyecto.NombrePrograma = request.NombrePrograma?.Trim() ?? string.Empty;
        proyecto.TipoPAP = request.TipoPAP;

        _context.Proyectos.Add(proyecto);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), proyecto.Id);
    }
}
