using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.RemoveRole;

public record RemoveRoleCommand : IRequest<Result>
{
    public string UserId { get; init; } = default!;
    public string RoleName { get; init; } = default!;
}

public class RemoveRoleCommandHandler : IRequestHandler<RemoveRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public RemoveRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(RemoveRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);
        if (role == null)
            return Result.Failure(["Rol no encontrado."]);

        var userRole = await _context.UserRoles
            .FirstOrDefaultAsync(ur => ur.UserId == request.UserId && ur.RoleId == role.Id, cancellationToken);

        if (userRole == null)
            return Result.Success(); // Idempotente — ya no tenía el rol

        _context.UserRoles.Remove(userRole);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
