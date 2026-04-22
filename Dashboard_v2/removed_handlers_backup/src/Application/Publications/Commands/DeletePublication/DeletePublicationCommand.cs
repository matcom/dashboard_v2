using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Publications.Commands.DeletePublication;

/// <summary>
/// Elimina una publicación y todas sus relaciones de autoría.<br/>
/// Solo puede hacerlo un usuario que sea autor de esa publicación.
/// </summary>
public record DeletePublicationCommand(string Id) : IRequest<Result>;

public class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand, Result>
{
    private readonly IPublicationService _service;

    public DeletePublicationCommandHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<Result> Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        return _service.DeleteAsync(request.Id, cancellationToken);
    }
}
