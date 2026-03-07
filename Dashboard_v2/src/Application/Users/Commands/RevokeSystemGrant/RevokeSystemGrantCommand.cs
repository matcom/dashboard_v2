using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Users.Commands.RevokeSystemGrant;

[Authorize(SystemPermission = SystemPermissions.RevokeSystemPerms)]
public record RevokeSystemGrantCommand(int GrantId) : IRequest;

public class RevokeSystemGrantCommandHandler : IRequestHandler<RevokeSystemGrantCommand>
{
    private readonly IPermissionService _permissionService;

    public RevokeSystemGrantCommandHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task Handle(RevokeSystemGrantCommand request, CancellationToken cancellationToken)
    {
        await _permissionService.RevokeSystemGrantAsync(request.GrantId, cancellationToken);
    }
}
