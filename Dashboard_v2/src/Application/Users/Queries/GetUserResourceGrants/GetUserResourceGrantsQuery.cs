using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Users.Queries.GetUserResourceGrants;

public record ResourceGrantDto
{
    public int GrantId { get; init; }
    public int ResourceId { get; init; }
    public string ResourceType { get; init; } = default!;
    public string ResourceTitle { get; init; } = default!;
    public string PermissionName { get; init; } = default!;
    public bool IsActive { get; init; }
    public DateTimeOffset? ExpiresAt { get; init; }
    public DateTimeOffset GrantedAt { get; init; }
}

/// <summary>Devuelve todos los grants de un usuario (para mostrar en su perfil de permisos).</summary>
[Authorize(SystemPermission = SystemPermissions.ViewGrants)]
public record GetUserResourceGrantsQuery(string UserId) : IRequest<List<ResourceGrantDto>>;

public class GetUserResourceGrantsQueryHandler : IRequestHandler<GetUserResourceGrantsQuery, List<ResourceGrantDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUserResourceGrantsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<ResourceGrantDto>> Handle(GetUserResourceGrantsQuery request, CancellationToken cancellationToken)
    {
        var grants = await _context.ResourceGrants
            .AsNoTracking()
            .Include(g => g.Resource)
            .Include(g => g.Permission)
            .Where(g => g.UserId == request.UserId && g.IsActive)
            .OrderBy(g => g.Resource.Type)
            .ToListAsync(cancellationToken);

        var result = new List<ResourceGrantDto>();

        foreach (var g in grants)
        {
            // Buscar el título según el tipo de recurso
            var title = await GetResourceTitleAsync(g.ResourceId, g.Resource.Type, cancellationToken);

            result.Add(new ResourceGrantDto
            {
                GrantId = g.Id,
                ResourceId = g.ResourceId,
                ResourceType = g.Resource.Type,
                ResourceTitle = title,
                PermissionName = g.Permission.Name,
                IsActive = g.IsActive,
                ExpiresAt = g.ExpiresAt,
                GrantedAt = g.GrantedAt
            });
        }

        return result;
    }

    private async Task<string> GetResourceTitleAsync(int resourceId, string resourceType, CancellationToken ct)
    {
        if (resourceType == "Publication")
        {
            var title = await _context.Publications
                .AsNoTracking()
                .Where(p => p.ResourceId == resourceId)
                .Select(p => p.Title)
                .FirstOrDefaultAsync(ct);
            return title ?? $"Publicación #{resourceId}";
        }

        return $"{resourceType} #{resourceId}";
    }
}
