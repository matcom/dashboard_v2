using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Publications;

namespace Dashboard_v2.Application.Publications.Queries.GetMyPublications;

/// <summary>
/// Retorna todas las publicaciones donde el usuario autenticado figura como autor.
/// Si el usuario no tiene perfil de autor todavía, retorna lista vacía.
/// </summary>
public record GetMyPublicationsQuery : IRequest<List<PublicationDto>>;

public class GetMyPublicationsQueryHandler : IRequestHandler<GetMyPublicationsQuery, List<PublicationDto>>
{
    private readonly IPublicationService _service;

    public GetMyPublicationsQueryHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<List<PublicationDto>> Handle(GetMyPublicationsQuery request, CancellationToken cancellationToken)
    {
        return _service.GetMyPublicationsAsync(cancellationToken);
    }
}
