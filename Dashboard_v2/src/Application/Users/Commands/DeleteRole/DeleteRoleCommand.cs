using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.DeleteRole;

public record DeleteRoleCommand(string RoleId) : IRequest;

[Authorize(SystemPermission = SystemPermissions.DeleteRoles)]
public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public DeleteRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
    {
        var role = await _context.Roles
            .FirstOrDefaultAsync(r => r.Id == request.RoleId, cancellationToken)
            ?? throw new NotFoundException("Rol", request.RoleId);

        if (role.Name == Domain.Constants.Roles.Administrator)
            throw new ForbiddenAccessException();

        _context.Roles.Remove(role);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
