using Dashboard_v2.Application.Events;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Events.Queries.GetEventTypes;

public record GetEventTypesQuery : IRequest<List<EventTypeDto>>;

public class GetEventTypesQueryHandler : IRequestHandler<GetEventTypesQuery, List<EventTypeDto>>
{
    public Task<List<EventTypeDto>> Handle(GetEventTypesQuery request, CancellationToken cancellationToken)
    {
        var result = Enum.GetValues<EventTypeEnum>()
            .Select(e => new EventTypeDto((int)e, e.ToString()))
            .ToList();
        return Task.FromResult(result);
    }
}
