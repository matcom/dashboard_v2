using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.AreasDelConocimiento.Commands.DeleteAreaDelConocimiento;

public record DeleteAreaDelConocimientoCommand(string Id) : IRequest<Result>;

public class DeleteAreaDelConocimientoCommandHandler : IRequestHandler<DeleteAreaDelConocimientoCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAreaDelConocimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAreaDelConocimientoCommand request, CancellationToken cancellationToken)
    {
        var entity = await _context.AreasDelConocimiento
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Área del conocimiento no encontrada."]);

        _context.AreasDelConocimiento.Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
