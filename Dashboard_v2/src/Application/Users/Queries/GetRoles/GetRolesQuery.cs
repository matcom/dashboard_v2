using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Queries.GetRoles;

public record RoleDto
{
    public string Id { get; init; } = default!;
    public string Name { get; init; } = default!;
    public int UserCount { get; init; }
    public List<RolePermGrantDto> SystemPermissions { get; init; } = [];
}

public record RolePermGrantDto
{
    public int GrantId { get; init; }
    public string Permission { get; init; } = default!;
}

[Authorize(SystemPermission = SystemPermissions.ViewRoles)]
public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IApplicationDbContext _context;

    public GetRolesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => new RoleDto
            {
                Id   = r.Id,
                Name = r.Name,
                UserCount = r.UserRoles.Count,
                SystemPermissions = r.SystemPermissions
                    .Where(sp => sp.IsActive)
                    .Select(sp => new RolePermGrantDto { GrantId = sp.Id, Permission = sp.Permission })
                    .ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
