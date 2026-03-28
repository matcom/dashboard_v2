using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Queries.GetUsers;

public record UserWithRolesDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    public bool IsActive { get; init; }
    public List<string> Roles { get; init; } = [];
}

public record GetUsersQuery : IRequest<List<UserWithRolesDto>>;

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, List<UserWithRolesDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUsersQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UserWithRolesDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        return await _context.Users
            .AsNoTracking()
            .Select(u => new UserWithRolesDto
            {
                Id = u.Id,
                UserName = u.UserName,
                Email = u.Email,
                IsActive = u.IsActive,
                Roles = u.UserRoles.Select(ur => ur.Role.Name).ToList()
            })
            .OrderBy(u => u.UserName)
            .ToListAsync(cancellationToken);
    }
}
