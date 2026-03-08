using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.UpdateUserRoles;

/// <summary>
/// Reemplaza todos los roles de un usuario por la lista indicada.
/// </summary>
[Authorize(SystemPermission = SystemPermissions.ManageUsers)]
public record UpdateUserRolesCommand : IRequest
{
    public string UserId { get; init; } = default!;
    public List<string> RoleIds { get; init; } = [];
}

public class UpdateUserRolesCommandHandler : IRequestHandler<UpdateUserRolesCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdateUserRolesCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdateUserRolesCommand request, CancellationToken cancellationToken)
    {
        var userExists = await _context.Users.AnyAsync(u => u.Id == request.UserId, cancellationToken);
        if (!userExists)
            throw new NotFoundException("User", request.UserId);

        // Eliminar roles actuales
        var current = _context.UserRoles.Where(ur => ur.UserId == request.UserId);
        _context.UserRoles.RemoveRange(current);

        // Añadir nuevos roles (solo los que existen)
        var validRoleIds = await _context.Roles
            .Where(r => request.RoleIds.Contains(r.Id))
            .Select(r => r.Id)
            .ToListAsync(cancellationToken);

        foreach (var roleId in validRoleIds)
        {
            _context.UserRoles.Add(new UserRole { UserId = request.UserId, RoleId = roleId });
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
