using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.RevokeSystemPermFromRole;

public record RevokeSystemPermFromRoleCommand(int GrantId) : IRequest;

[Authorize(SystemPermission = SystemPermissions.ManageRolePerms)]
public class RevokeSystemPermFromRoleCommandHandler : IRequestHandler<RevokeSystemPermFromRoleCommand>
{
    private readonly IApplicationDbContext _context;

    public RevokeSystemPermFromRoleCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(RevokeSystemPermFromRoleCommand request, CancellationToken cancellationToken)
    {
        var grant = await _context.RoleSystemPermissions
            .FirstOrDefaultAsync(rsp => rsp.Id == request.GrantId, cancellationToken)
            ?? throw new NotFoundException("RoleSystemPermission", request.GrantId.ToString());

        grant.IsActive = false;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
