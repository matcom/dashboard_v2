using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;

namespace Dashboard_v2.Application.Events.Queries.GetMyEvents;

/// <summary>Retorna los eventos en los que el usuario autenticado tiene al menos una presentación.</summary>
public record GetMyEventsQuery : IRequest<List<EventDto>>;

public class GetMyEventsQueryHandler : IRequestHandler<GetMyEventsQuery, List<EventDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetMyEventsQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<List<EventDto>> Handle(GetMyEventsQuery request, CancellationToken cancellationToken)
    {
        var authorId = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId == _currentUser.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (authorId is null)
            return [];

        return await _context.Events
            .AsNoTracking()
            .Where(e => e.Presentations.Any(p =>
                p.AuthorPresentations.Any(ap => ap.AuthorId == authorId)))
            .Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                CountryId = e.CountryId,
                CountryName = e.Country.Name,
                EventTypeId = e.EventTypeId,
                EventTypeName = e.EventType.Name,
                Institutions = e.Institutions,
                PresentationCount = e.Presentations.Count(p =>
                    p.AuthorPresentations.Any(ap => ap.AuthorId == authorId)),
            })
            .OrderBy(e => e.Name)
            .ToListAsync(cancellationToken);
    }
}
