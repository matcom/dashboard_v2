using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using RolesEnum = global::Dashboard_v2.Domain.Enums.Roles;

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
        if (!Enum.TryParse<RolesEnum>(request.RoleName, out var roleEnum) || roleEnum == RolesEnum.None)
            return Result.Failure(["Rol no válido."]);

        var user = await _context.Users.FindAsync([request.UserId], cancellationToken);
        if (user == null)
            return Result.Failure(["Usuario no encontrado."]);

        var alreadyAssigned = await _context.UserRoles
            .AnyAsync(ur => ur.UserId == request.UserId && ur.Role == roleEnum, cancellationToken);
        if (alreadyAssigned)
            return Result.Failure(["El usuario ya tiene este rol asignado."]);

        _context.UserRoles.Add(new UserRole { UserId = request.UserId, Role = roleEnum });
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
