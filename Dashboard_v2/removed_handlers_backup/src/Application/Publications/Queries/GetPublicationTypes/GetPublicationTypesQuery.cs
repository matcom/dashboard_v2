using Dashboard_v2.Application.Publications;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Publications.Queries.GetPublicationTypes;

/// <summary>Retorna todos los tipos de publicación disponibles (desde el enum PublicationType).</summary>
public record GetPublicationTypesQuery : IRequest<List<PublicationTypeDto>>;

public class GetPublicationTypesQueryHandler : IRequestHandler<GetPublicationTypesQuery, List<PublicationTypeDto>>
{
    private readonly IPublicationService _service;

    public GetPublicationTypesQueryHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<List<PublicationTypeDto>> Handle(GetPublicationTypesQuery request, CancellationToken cancellationToken)
        => _service.GetPublicationTypesAsync();
}
