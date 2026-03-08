using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Users.Commands.CreateUser;

[Authorize(SystemPermission = SystemPermissions.CreateUsers)]
public record CreateUserCommand : IRequest<Result>
{
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public string Password { get; init; } = default!;
    public List<string> RoleIds { get; init; } = [];
}

public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Result>
{
    private readonly IIdentityService _identityService;
    private readonly IApplicationDbContext _context;

    public CreateUserCommandHandler(IIdentityService identityService, IApplicationDbContext context)
    {
        _identityService = identityService;
        _context = context;
    }

    public async Task<Result> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var (result, userId) = await _identityService.CreateUserAsync(
            request.UserName, request.Email, request.Password);

        if (!result.Succeeded) return result;

        foreach (var roleId in request.RoleIds.Distinct())
        {
            if (_context.Roles.Any(r => r.Id == roleId))
            {
                _context.UserRoles.Add(new Domain.Entities.UserRole
                {
                    UserId = userId,
                    RoleId = roleId
                });
            }
        }

        if (request.RoleIds.Count > 0)
            await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
