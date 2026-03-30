using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;

namespace Dashboard_v2.Application.Events.Queries.GetAllEvents;

/// <summary>Retorna todos los eventos registrados (para usar como lookup en formularios).</summary>
public record GetAllEventsQuery : IRequest<List<EventDto>>;

public class GetAllEventsQueryHandler : IRequestHandler<GetAllEventsQuery, List<EventDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAllEventsQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<EventDto>> Handle(GetAllEventsQuery request, CancellationToken cancellationToken)
        => await _context.Events
            .AsNoTracking()
            .OrderBy(e => e.Name)
            .Select(e => new EventDto
            {
                Id = e.Id,
                Name = e.Name,
                CountryId = e.CountryId,
                CountryName = e.Country.Name,
                EventType = (int)e.EventType,
                Institutions = e.Institutions,
                PresentationCount = e.Presentations.Count,
            })
            .ToListAsync(cancellationToken);
}
