using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
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
}

public record GetPublicationsQuery : IRequest<PaginatedList<PublicationDto>>
{
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 20;
    public string? Search { get; init; }
}

public class GetPublicationsQueryHandler : IRequestHandler<GetPublicationsQuery, PaginatedList<PublicationDto>>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaginatedList<PublicationDto>> Handle(GetPublicationsQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Publications
            .AsNoTracking()
            .Include(p => p.PublicationType)
            .Include(p => p.Resource)
            .AsQueryable();

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
                Created = p.Created
            });

        return await PaginatedList<PublicationDto>.CreateAsync(projected, request.PageNumber, request.PageSize, cancellationToken);
    }
}
