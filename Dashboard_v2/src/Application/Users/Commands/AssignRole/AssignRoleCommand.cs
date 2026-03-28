using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.AssignRole;

public record AssignRoleCommand : IRequest<Result>
{
    public string UserId { get; init; } = default!;
    public string RoleName { get; init; } = default!;
}

public class AssignRoleCommandHandler : IRequestHandler<AssignRoleCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public AssignRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(AssignRoleCommand request, CancellationToken cancellationToken)
    {
        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);
        if (user == null)
            return Result.Failure(["Usuario no encontrado."]);

        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Name == request.RoleName, cancellationToken);
        if (role == null)
            return Result.Failure(["Rol no encontrado."]);

        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.RoleId == role.Id, cancellationToken);
        if (alreadyAssigned)
            return Result.Failure(["El usuario ya tiene este rol asignado."]);

        _context.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = role.Id });
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
