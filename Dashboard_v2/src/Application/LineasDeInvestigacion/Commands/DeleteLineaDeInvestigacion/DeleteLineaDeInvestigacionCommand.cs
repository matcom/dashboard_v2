using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.LineasDeInvestigacion.Commands.DeleteLineaDeInvestigacion;

public record DeleteLineaDeInvestigacionCommand(string Id) : IRequest<Result>;

public class DeleteLineaDeInvestigacionCommandHandler : IRequestHandler<DeleteLineaDeInvestigacionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteLineaDeInvestigacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteLineaDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.LineasDeInvestigacion
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Línea de investigación no encontrada."]);

        _context.LineasDeInvestigacion.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
