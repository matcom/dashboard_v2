using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Queries.GetUsers;

public record UserListDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public List<string> Roles { get; init; } = [];
}

[Authorize(SystemPermission = SystemPermissions.ViewUsers)]
public record GetUsersQuery : IRequest<List<UserListDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserListDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserListDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.UserName)
            .Select(u => new
            {
                u.Id, u.UserName, u.Email, u.IsActive, u.CreatedAt,
                Roles = _context.UserRoles
                    .Where(ur => ur.UserId == u.Id)
                    .Select(ur => ur.Role.Name)
                    .ToList()
            })
            .ToListAsync(cancellationToken);

        return users.Select(u => new UserListDto
        {
            Id = u.Id,
            UserName = u.UserName,
            Email = u.Email,
            IsActive = u.IsActive,
            CreatedAt = u.CreatedAt,
            Roles = u.Roles
        }).ToList();
    }
}
