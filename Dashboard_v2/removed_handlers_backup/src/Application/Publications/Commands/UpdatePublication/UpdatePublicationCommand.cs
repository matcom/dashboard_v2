using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Publications.Commands.UpdatePublication;

/// <summary>
/// Actualiza título, datos, tipo y coautores de una publicación.<br/>
/// Solo el usuario que sea autor de la publicación puede modificarla.
/// </summary>
public record UpdatePublicationCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string PublicationData { get; init; } = default!;
    public PublicationType PublicationType { get; init; }
    public string? UrlDoi { get; init; }
    /// <summary>IDs de autores ya existentes en BD que deben figurar como coautores.</summary>
    public List<string> AdditionalAuthorIds { get; init; } = [];
    /// <summary>Nombres de coautores nuevos (no existían en la BD).</summary>
    public List<string> AdditionalAuthorNames { get; init; } = [];
    /// <summary>
    /// IDs de usuarios registrados que serán coautores.<br/>
    /// Si el usuario ya tiene perfil de autor se reutiliza; si no, se crea automáticamente.
    /// </summary>
    public List<string> AdditionalUserIds { get; init; } = [];

    // ── Campos de especialización ─────────────────────────────────────────────────────────────
    public string? Index { get; init; }
    public string? DataBase { get; init; }
    public int? Group { get; init; }
    public string? Cuartil { get; init; }    /// <summary>ID del proyecto del que deriva esta publicación. Null para desvincular.</summary>
    public string? ProyectoId { get; init; }}

public class UpdatePublicationCommandHandler : IRequestHandler<UpdatePublicationCommand, Result>
{
    private readonly IPublicationService _service;

    public UpdatePublicationCommandHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<Result> Handle(UpdatePublicationCommand request, CancellationToken cancellationToken)
    {
        var req = new UpdatePublicationRequest
        {
            Id = request.Id,
            Title = request.Title,
            PublicationData = request.PublicationData,
            PublicationType = request.PublicationType,
            UrlDoi = request.UrlDoi,
            AdditionalAuthorIds = request.AdditionalAuthorIds,
            AdditionalAuthorNames = request.AdditionalAuthorNames,
            AdditionalUserIds = request.AdditionalUserIds,
            Index = request.Index,
            DataBase = request.DataBase,
            Group = request.Group,
            Cuartil = request.Cuartil,
            ProyectoId = request.ProyectoId
        };

        return _service.UpdateAsync(req, cancellationToken);
    }
}
