using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Roles.Queries.GetRoles;

public record RoleDto
{
    public string Name { get; init; } = default!;
}

public record GetRolesQuery : IRequest<List<RoleDto>>;

public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
    {
        var roles = Enum.GetValues<Roles>()
            .Where(r => r != Roles.None && r != Roles.Superuser)
            .Select(r => new RoleDto { Name = r.ToString() })
            .OrderBy(r => r.Name)
            .ToList();

        return Task.FromResult(roles);
    }
}
