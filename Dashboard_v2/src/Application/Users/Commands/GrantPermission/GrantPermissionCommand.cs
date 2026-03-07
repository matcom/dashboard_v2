using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Commands.GrantPermission;

[Authorize(SystemPermission = SystemPermissions.GrantResourcePerms)]
public record GrantPermissionCommand : IRequest<int>
{
    public string TargetUserId { get; init; } = default!;
    public int ResourceId { get; init; }
    public string PermissionName { get; init; } = default!;
    public DateTimeOffset? ExpiresAt { get; init; }
}

public class GrantPermissionCommandHandler : IRequestHandler<GrantPermissionCommand, int>
{
    private readonly IPermissionService _permissionService;
    private readonly IUser _currentUser;
    private readonly IApplicationDbContext _context;

    public GrantPermissionCommandHandler(IPermissionService permissionService, IUser currentUser, IApplicationDbContext context)
    {
        _permissionService = permissionService;
        _currentUser = currentUser;
        _context = context;
    }

    public async Task<int> Handle(GrantPermissionCommand request, CancellationToken cancellationToken)
    {
        var targetUser = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.TargetUserId, cancellationToken)
            ?? throw new NotFoundException("User", request.TargetUserId);

        if (targetUser.IsActive)
            throw new Dashboard_v2.Application.Common.Exceptions.ValidationException([new ValidationFailure("TargetUserId",
                "Solo se pueden asignar permisos a usuarios desactivados. Desactive el usuario primero.")]);

        return await _permissionService.GrantPermissionAsync(
            request.TargetUserId,
            request.ResourceId,
            request.PermissionName,
            _currentUser.Id!,
            request.ExpiresAt,
            null,
            cancellationToken);
    }
}
