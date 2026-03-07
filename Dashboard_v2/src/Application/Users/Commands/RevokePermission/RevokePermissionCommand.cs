using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Users.Commands.RevokePermission;

[Authorize(SystemPermission = SystemPermissions.RevokeResourcePerms)]
public record RevokePermissionCommand(int GrantId) : IRequest;

public class RevokePermissionCommandHandler : IRequestHandler<RevokePermissionCommand>
{
    private readonly IPermissionService _permissionService;

    public RevokePermissionCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task Handle(RevokePermissionCommand request, CancellationToken cancellationToken)
    {
        await _permissionService.DeactivateGrantAsync(request.GrantId, cancellationToken);
    }
}
