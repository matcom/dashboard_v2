using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Awards.Commands.DeleteAward;

public record DeleteAwardCommand(int Id) : IRequest<Result>;

public class DeleteAwardCommandHandler : IRequestHandler<DeleteAwardCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DeleteAwardCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeleteAwardCommand request, CancellationToken cancellationToken)
    {
        var userAwarded = await _context.UserAwardeds
            .FirstOrDefaultAsync(ua => ua.Id == request.Id, cancellationToken);

        if (userAwarded is null)
            return Result.Failure(["Premio no encontrado."]);

        if (userAwarded.UserId != _currentUser.Id)
            return Result.Failure(["No tienes permiso para eliminar este premio."]);

        _context.UserAwardeds.Remove(userAwarded);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
