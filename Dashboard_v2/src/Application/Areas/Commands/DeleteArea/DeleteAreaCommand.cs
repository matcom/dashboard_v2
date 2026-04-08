using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Areas.Commands.DeleteArea;

public record DeleteAreaCommand(string Id) : IRequest<Result>;

public class DeleteAreaCommandHandler : IRequestHandler<DeleteAreaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteAreaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteAreaCommand request, CancellationToken cancellationToken)
    {
        var area = await _context.Areas
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (area is null)
            return Result.Failure(["Área no encontrada."]);

        _context.Areas.Remove(area);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
