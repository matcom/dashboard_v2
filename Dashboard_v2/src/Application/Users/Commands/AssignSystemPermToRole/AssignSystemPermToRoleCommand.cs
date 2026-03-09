using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.AssignSystemPermToRole;

public record AssignSystemPermToRoleCommand(string RoleId, string Permission) : IRequest<int>;

[Authorize(SystemPermission = SystemPermissions.ManageRolePerms)]
public class AssignSystemPermToRoleCommandHandler : IRequestHandler<AssignSystemPermToRoleCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AssignSystemPermToRoleCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(AssignSystemPermToRoleCommand request, CancellationToken cancellationToken)
    {
        if (!await _context.Roles.AnyAsync(r => r.Id == request.RoleId, cancellationToken))
            throw new NotFoundException("Rol", request.RoleId);

        if (!SystemPermissions.AllPermissions.Contains(request.Permission))
            throw new NotFoundException("Permission", request.Permission);

        // Si ya existe (inactivo), reactivar
        var existing = await _context.RoleSystemPermissions
            .FirstOrDefaultAsync(rsp => rsp.RoleId == request.RoleId && rsp.Permission == request.Permission, cancellationToken);

        if (existing != null)
        {
            existing.IsActive = true;
            existing.GrantedAt = DateTimeOffset.UtcNow;
            existing.GrantedBy = _currentUser.Id;
            await _context.SaveChangesAsync(cancellationToken);
            return existing.Id;
        }

        var grant = new RoleSystemPermission
        {
            RoleId     = request.RoleId,
            Permission = request.Permission,
            IsActive   = true,
            GrantedAt  = DateTimeOffset.UtcNow,
            GrantedBy  = _currentUser.Id
        };
        _context.RoleSystemPermissions.Add(grant);
        await _context.SaveChangesAsync(cancellationToken);
        return grant.Id;
    }
}
