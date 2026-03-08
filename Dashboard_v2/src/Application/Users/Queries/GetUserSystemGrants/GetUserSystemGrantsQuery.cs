using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Users.Queries.GetUserSystemGrants;

public record SystemGrantDto
{
    public int GrantId { get; init; }
    public string Permission { get; init; } = default!;
    public string? PermissionLabel { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset GrantedAt { get; init; }
}

[Authorize(SystemPermission = SystemPermissions.ViewGrants)]
public record GetUserSystemGrantsQuery(string UserId) : IRequest<List<SystemGrantDto>>;

public class GetUserSystemGrantsQueryHandler : IRequestHandler<GetUserSystemGrantsQuery, List<SystemGrantDto>>
{
    private readonly IPermissionService _permissionService;

    public GetUserSystemGrantsQueryHandler(IPermissionService permissionService)
    {
        _permissionService = permissionService;
    }

    public async Task<List<SystemGrantDto>> Handle(GetUserSystemGrantsQuery request, CancellationToken cancellationToken)
    {
        var grants = await _permissionService.GetUserSystemGrantsAsync(request.UserId, cancellationToken);

        return grants.Select(g => new SystemGrantDto
        {
            GrantId        = g.GrantId,
            Permission     = g.Permission,
            PermissionLabel = SystemPermissionLabels.Get(g.Permission),
            ExpiresAt      = g.ExpiresAt,
            GrantedAt      = g.GrantedAt
        }).ToList();
    }
}

/// <summary>Etiquetas legibles para cada permiso de sistema.</summary>
internal static class SystemPermissionLabels
{
    private static readonly Dictionary<string, string> _labels = new()
    {
        [SystemPermissions.ViewUsers]           = "Ver usuarios",
        [SystemPermissions.CreateUsers]         = "Crear usuarios",
        [SystemPermissions.ManageUsers]         = "Gestionar usuarios (roles / estado)",
        [SystemPermissions.ViewGrants]          = "Ver permisos de usuarios",
        [SystemPermissions.GrantSystemPerms]    = "Asignar permisos de sistema",
        [SystemPermissions.GrantResourcePerms]  = "Asignar permisos de recurso",
        [SystemPermissions.RevokeSystemPerms]   = "Revocar permisos de sistema",
        [SystemPermissions.RevokeResourcePerms] = "Revocar permisos de recurso",
        [SystemPermissions.ViewAllPublications]   = "Ver todas las publicaciones",
        [SystemPermissions.CreatePublications]    = "Crear publicaciones",
        [SystemPermissions.EditAnyPublication]    = "Editar cualquier publicación",
        [SystemPermissions.DeleteAnyPublication]  = "Eliminar cualquier publicación",
        [SystemPermissions.All]                 = "Acceso completo al sistema",
    };

    public static string Get(string perm) =>
        _labels.TryGetValue(perm, out var label) ? label : perm;
}
