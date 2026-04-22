using RolesEnum = global::Dashboard_v2.Domain.Enums.Roles;
using Dashboard_v2.Application.Roles;

namespace Dashboard_v2.Application.Roles.Queries.GetRoles;

/// <summary>Consulta MediatR que retorna los roles asignables (excluye <c>None</c> y <c>Superuser</c>).</summary>
public record GetRolesQuery : IRequest<List<RoleDto>>;

/// <summary>
/// Devuelve los roles disponibles para asignar a usuarios ordinarios.<br/>
/// Los roles se obtienen del enum en memoria — no requiere consulta a la BD.<br/>
/// Excluye <c>None</c> (valor nulo del enum) y <c>Superuser</c> (solo se asigna en el seed inicial).
/// </summary>
public class GetRolesQueryHandler : IRequestHandler<GetRolesQuery, List<RoleDto>>
{
    private readonly IRoleService _service;

    public GetRolesQueryHandler(IRoleService service)
    {
        _service = service;
    }

    public Task<List<RoleDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        => _service.GetAssignableRolesAsync();
}
