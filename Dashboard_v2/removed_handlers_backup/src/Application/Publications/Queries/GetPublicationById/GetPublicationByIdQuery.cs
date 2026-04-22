using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Publications;

namespace Dashboard_v2.Application.Publications.Queries.GetPublicationById;

/// <summary>
/// Retorna una publicación por ID, solo si el usuario autenticado es uno de sus autores.
/// Devuelve null si la publicación no existe o el usuario no es autor (evita revelar existencia).
/// </summary>
public record GetPublicationByIdQuery(string Id) : IRequest<PublicationDto?>;

public class GetPublicationByIdQueryHandler : IRequestHandler<GetPublicationByIdQuery, PublicationDto?>
{
    private readonly IPublicationService _service;

    public GetPublicationByIdQueryHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<PublicationDto?> Handle(GetPublicationByIdQuery request, CancellationToken cancellationToken)
    {
        return _service.GetByIdAsync(request.Id, cancellationToken);
    }
}
