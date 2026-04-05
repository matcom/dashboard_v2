using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.GruposDeInvestigacion.Commands.DeleteGrupoDeInvestigacion;

public record DeleteGrupoDeInvestigacionCommand(string Id) : IRequest<Result>;

public class DeleteGrupoDeInvestigacionCommandHandler : IRequestHandler<DeleteGrupoDeInvestigacionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteGrupoDeInvestigacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteGrupoDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        var grupo = await _context.GruposDeInvestigacion
            .FirstOrDefaultAsync(g => g.Id == request.Id, cancellationToken);

        if (grupo is null)
            return Result.Failure(["Grupo de investigación no encontrado."]);

        _context.GruposDeInvestigacion.Remove(grupo);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
