using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.GruposDeInvestigacion.Commands.CreateGrupoDeInvestigacion;

public record CreateGrupoDeInvestigacionCommand : IRequest<(Result Result, string? Id)>
{
    public string Nombre { get; init; } = default!;
    public string AreaId { get; init; } = default!;
}

public class CreateGrupoDeInvestigacionCommandHandler
    : IRequestHandler<CreateGrupoDeInvestigacionCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateGrupoDeInvestigacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Result Result, string? Id)> Handle(
        CreateGrupoDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (Result.Failure(["El nombre es obligatorio."]), null);

        if (!await _context.Areas.AnyAsync(a => a.Id == request.AreaId, cancellationToken))
            return (Result.Failure(["El área indicada no existe."]), null);

        var grupo = new GrupoDeInvestigacion
        {
            Nombre = request.Nombre.Trim(),
            AreaId = request.AreaId
        };

        _context.GruposDeInvestigacion.Add(grupo);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), grupo.Id);
    }
}
