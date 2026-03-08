using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Queries.GetPublications;

public record PublicationDto
{
    public int Id { get; init; }
    public int ResourceId { get; init; }
    public string Title { get; init; } = default!;
    public string? AuthorRelation { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public int PublicationTypeId { get; init; }
    public string PublicationTypeName { get; init; } = default!;
    public bool IsJournal { get; init; }
    public bool IsIndexedPublication { get; init; }
    public string OwnerId { get; init; } = default!;
    public DateTimeOffset Created { get; init; }
    /// <summary>El usuario actual puede editar esta publicación (sistema o recurso).</summary>
    public bool CanEdit { get; init; }
    /// <summary>El usuario actual puede eliminar esta publicación (sistema o recurso).</summary>
    public bool CanDelete { get; init; }
}

// Solo requiere autenticación; el handler filtra según los permisos del usuario.
[Authorize]
public record GetPublicationsQuery : IRequest<PaginatedList<PublicationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
}

public class GetPublicationsQueryHandler : IRequestHandler<GetPublicationsQuery, PaginatedList<PublicationDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IPermissionService _permissionService;

    public GetPublicationsQueryHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IPermissionService permissionService)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task<PaginatedList<PublicationDto>> Handle(GetPublicationsQuery request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.Id!;
        var now    = DateTimeOffset.UtcNow;

        // Determinar nivel de acceso global
        var canViewAll   = await _permissionService.HasSystemPermissionAsync(userId, SystemPermissions.ViewAllPublications,  cancellationToken);
        var canEditAny   = await _permissionService.HasSystemPermissionAsync(userId, SystemPermissions.EditAnyPublication,   cancellationToken);
        var canDeleteAny = await _permissionService.HasSystemPermissionAsync(userId, SystemPermissions.DeleteAnyPublication, cancellationToken);

        // Para usuarios sin view_all: precargar IDs de recursos accesibles por grants
        List<int> accessibleResourceIds = [];
        List<int> editableResourceIds   = [];
        List<int> deletableResourceIds  = [];

        if (!canViewAll)
        {
            accessibleResourceIds = await _context.ResourceGrants
                .AsNoTracking()
                .Where(rg => rg.UserId == userId && rg.IsActive &&
                             (rg.ExpiresAt == null || rg.ExpiresAt > now))
                .Select(rg => rg.ResourceId)
                .Distinct()
                .ToListAsync(cancellationToken);

            editableResourceIds = await _context.ResourceGrants
                .AsNoTracking()
                .Include(rg => rg.Permission)
                .Where(rg => rg.UserId == userId && rg.IsActive &&
                             (rg.ExpiresAt == null || rg.ExpiresAt > now) &&
                             (rg.Permission.Name == "write" || rg.Permission.Name == "admin"))
                .Select(rg => rg.ResourceId)
                .Distinct()
                .ToListAsync(cancellationToken);

            deletableResourceIds = await _context.ResourceGrants
                .AsNoTracking()
                .Include(rg => rg.Permission)
                .Where(rg => rg.UserId == userId && rg.IsActive &&
                             (rg.ExpiresAt == null || rg.ExpiresAt > now) &&
                             (rg.Permission.Name == "delete" || rg.Permission.Name == "admin"))
                .Select(rg => rg.ResourceId)
                .Distinct()
                .ToListAsync(cancellationToken);
        }

        var query = _context.Publications
            .AsNoTracking()
            .Include(p => p.PublicationType)
            .Include(p => p.Resource)
            .AsQueryable();

        // Filtrar: solo publicaciones a las que el usuario tiene acceso
        if (!canViewAll)
        {
            query = query.Where(p =>
                p.Resource.OwnerId == userId ||
                accessibleResourceIds.Contains(p.ResourceId));
        }

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var term = request.Search.ToLower();
            query = query.Where(p =>
                p.Title.ToLower().Contains(term) ||
                (p.AuthorRelation != null && p.AuthorRelation.ToLower().Contains(term)));
        }

        var projected = query
            .OrderByDescending(p => p.Created)
            .Select(p => new PublicationDto
            {
                Id = p.Id,
                ResourceId = p.ResourceId,
                Title = p.Title,
                AuthorRelation = p.AuthorRelation,
                PublicationDate = p.PublicationDate,
                PublicationTypeId = p.PublicationTypeId,
                PublicationTypeName = p.PublicationType.Name,
                IsJournal = _context.Journals.Any(j => j.PublicationId == p.Id),
                IsIndexedPublication = _context.IndexedPublications.Any(ip => ip.PublicationId == p.Id),
                OwnerId = p.Resource.OwnerId,
                Created = p.Created,
                CanEdit   = canEditAny   || p.Resource.OwnerId == userId || editableResourceIds.Contains(p.ResourceId),
                CanDelete = canDeleteAny || p.Resource.OwnerId == userId || deletableResourceIds.Contains(p.ResourceId),
            });

        return await PaginatedList<PublicationDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
