using Dashboard_v2.Application.Publications;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Publications.Queries.GetPublicationTypes;

/// <summary>Retorna todos los tipos de publicación disponibles (desde el enum).</summary>
public record GetPublicationTypesQuery : IRequest<List<PublicationTypeDto>>;

public class GetPublicationTypesQueryHandler : IRequestHandler<GetPublicationTypesQuery, List<PublicationTypeDto>>
{
    public Task<List<PublicationTypeDto>> Handle(GetPublicationTypesQuery request, CancellationToken cancellationToken)
    {
        var result = Enum.GetValues<PublicationType>()
            .Select(e => new PublicationTypeDto((int)e, e.ToString()))
            .ToList();
        return Task.FromResult(result);
    }
}
