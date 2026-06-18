using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Events;

namespace Dashboard_v2.Application.Events.Queries.GetEventTypes;

public record GetEventTypesQuery : IRequest<List<EventTypeDto>>;

public class GetEventTypesQueryHandler : IRequestHandler<GetEventTypesQuery, List<EventTypeDto>>
{
    private readonly IApplicationDbContext _context;

    public GetEventTypesQueryHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<List<EventTypeDto>> Handle(GetEventTypesQuery request, CancellationToken cancellationToken)
        => await _context.EventTypes
            .AsNoTracking()
            .OrderBy(t => t.Id)
            .Select(t => new EventTypeDto(t.Id, t.Name))
            .ToListAsync(cancellationToken);
}
