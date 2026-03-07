using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Constants;

namespace Dashboard_v2.Application.Auth.Queries.GetCurrentUser;

public record UserDto
{
    public string Id { get; init; } = default!;
    public string UserName { get; init; } = default!;
    public string Email { get; init; } = default!;
    /// <summary>
    /// Keys de permisos de sistema activos para este usuario.
    /// Los administradores reciben ["system.all"]. El frontend usa esta lista
    /// para ocultar/mostrar opciones de UI sin comprometer la seguridad del servidor.
    /// </summary>
    public IList<string> Permissions { get; init; } = [];
}

public record GetCurrentUserQuery : IRequest<UserDto?>;

public class GetCurrentUserQueryHandler : IRequestHandler<GetCurrentUserQuery, UserDto?>
{
    private readonly IUser _currentUser;
    private readonly IIdentityService _identityService;
    private readonly IPermissionService _permissionService;
    private readonly IApplicationDbContext _context;

    public GetCurrentUserQueryHandler(
        IUser currentUser,
        IIdentityService identityService,
        IPermissionService permissionService,
        IApplicationDbContext context)
    {
        _currentUser = currentUser;
        _identityService = identityService;
        _permissionService = permissionService;
        _context = context;
    }

    public async Task<UserDto?> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(_currentUser.Id))
            return null;

        var (userId, userName, email) = await _identityService.GetUserDetailsAsync(_currentUser.Id);

        if (userId == null)
            return null;

        // Los administradores tienen acceso total; los demás solo ven sus grants activos.
        List<string> permissions;
        if (await _identityService.IsInRoleAsync(_currentUser.Id, "Administrator"))
        {
            permissions = [SystemPermissions.All];
        }
        else
        {
            var grants = await _permissionService.GetUserSystemGrantsAsync(_currentUser.Id, cancellationToken);
            permissions = grants.Select(g => g.Permission).ToList();
        }

        // "publications.access" — permiso sintético que indica si el usuario puede ver la sección.
        // Lo tienen: usuarios con system.all / view_all, y usuarios con al menos un ResourceGrant sobre alguna publicación.
        if (!permissions.Contains(SystemPermissions.All) && !permissions.Contains(SystemPermissions.ViewAllPublications))
        {
            var now = DateTimeOffset.UtcNow;
            var hasPublicationGrants = await _context.ResourceGrants
                .AsNoTracking()
                .Join(_context.Publications, rg => rg.ResourceId, p => p.ResourceId, (rg, _) => rg)
                .AnyAsync(rg =>
                    rg.UserId == _currentUser.Id &&
                    rg.IsActive &&
                    (rg.ExpiresAt == null || rg.ExpiresAt > now),
                    cancellationToken);

            if (hasPublicationGrants)
                permissions.Add("publications.access");
        }
        else
        {
            // Tiene view_all o system.all: puede acceder a la sección
            if (!permissions.Contains("publications.access"))
                permissions.Add("publications.access");
        }

        return new UserDto
        {
            Id = userId,
            UserName = userName ?? string.Empty,
            Email = email ?? string.Empty,
            Permissions = permissions
        };
    }
}
