using Dashboard_v2.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Queries.GetPublicationById;

public record JournalDetailDto
{
    public string? Database { get; init; }
    public string? GroupName { get; init; }
    public string? Quartile { get; init; }
}

public record IndexedPublicationDetailDto
{
    public string? IndexName { get; init; }
}

public record PublicationDetailDto
{
    public int Id { get; init; }
    public int ResourceId { get; init; }
    public string Title { get; init; } = default!;
    public string? AuthorRelation { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public int PublicationTypeId { get; init; }
    public string PublicationTypeName { get; init; } = default!;
    public string OwnerId { get; init; } = default!;
    public JournalDetailDto? Journal { get; init; }
    public IndexedPublicationDetailDto? IndexedPublication { get; init; }
}

public record GetPublicationByIdQuery(int Id) : IRequest<PublicationDetailDto?>;

public class GetPublicationByIdQueryHandler : IRequestHandler<GetPublicationByIdQuery, PublicationDetailDto?>
{
    private readonly IApplicationDbContext _context;

    public GetPublicationByIdQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PublicationDetailDto?> Handle(GetPublicationByIdQuery request, CancellationToken cancellationToken)
    {
        var pub = await _context.Publications
            .AsNoTracking()
            .Include(p => p.PublicationType)
            .Include(p => p.Resource)
            .Include(p => p.Journal)
            .Include(p => p.IndexedPublication)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (pub == null) return null;

        return new PublicationDetailDto
        {
            Id = pub.Id,
            ResourceId = pub.ResourceId,
            Title = pub.Title,
            AuthorRelation = pub.AuthorRelation,
            PublicationDate = pub.PublicationDate,
            PublicationTypeId = pub.PublicationTypeId,
            PublicationTypeName = pub.PublicationType.Name,
            OwnerId = pub.Resource.OwnerId,
            Journal = pub.Journal == null ? null : new JournalDetailDto
            {
                Database = pub.Journal.Database,
                GroupName = pub.Journal.GroupName,
                Quartile = pub.Journal.Quartile
            },
            IndexedPublication = pub.IndexedPublication == null ? null : new IndexedPublicationDetailDto
            {
                IndexName = pub.IndexedPublication.IndexName
            }
        };
    }
}
