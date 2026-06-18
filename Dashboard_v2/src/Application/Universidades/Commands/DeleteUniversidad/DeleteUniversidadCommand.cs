using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Universidades.Commands.DeleteUniversidad;

public record DeleteUniversidadCommand(string Id) : IRequest<Result>;

public class DeleteUniversidadCommandHandler : IRequestHandler<DeleteUniversidadCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteUniversidadCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(DeleteUniversidadCommand request, CancellationToken cancellationToken)
    {
        var universidad = await _context.Universidades
            .FirstOrDefaultAsync(u => u.Id == request.Id, cancellationToken);

        if (universidad is null)
            return Result.Failure(["Universidad no encontrada."]);

        _context.Universidades.Remove(universidad);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
