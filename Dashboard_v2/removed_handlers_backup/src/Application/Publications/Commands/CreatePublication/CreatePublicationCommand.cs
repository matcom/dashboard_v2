using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Publications.Commands.CreatePublication;

/// <summary>
/// Crea una nueva publicación y registra al Profesor actual como uno de sus autores.<br/>
/// Si el usuario no tiene perfil de autor, se crea automáticamente usando su nombre completo.<br/>
/// Los autores adicionales se pasan como lista de nombres; se crean como autores sin cuenta.
/// </summary>
public record CreatePublicationCommand : IRequest<(Result Result, string? PublicationId)>
{
    public string Title { get; init; } = default!;
    public string PublicationData { get; init; } = default!;
    public PublicationType PublicationType { get; init; }
    public string? UrlDoi { get; init; }
    /// <summary>IDs de autores ya existentes en BD que son coautores.</summary>
    public List<string> AdditionalAuthorIds { get; init; } = [];
    /// <summary>Nombres de coautores nuevos (no existían en la BD).</summary>
    public List<string> AdditionalAuthorNames { get; init; } = [];
    /// <summary>
    /// IDs de usuarios registrados que serán coautores.<br/>
    /// Si el usuario ya tiene perfil de autor se reutiliza; si no, se crea automáticamente.
    /// </summary>
    public List<string> AdditionalUserIds { get; init; } = [];

    // ── Campos de especialización ─────────────────────────────────────────────────────────────
    /// <summary>Indexación. Obligatorio para tipos Libro, Monografía, Capítulo, Artículo de Divulgación.</summary>
    public string? Index { get; init; }
    /// <summary>Base de datos donde aparece la revista. Obligatorio para tipo Diario.</summary>
    public string? DataBase { get; init; }
    /// <summary>Grupo de la revista (1–4). Obligatorio para tipo Diario.</summary>
    public int? Group { get; init; }
    /// <summary>Cuartil Scimago. Obligatorio para tipo Diario con grupo 1.</summary>
    public string? Cuartil { get; init; }
    /// <summary>ID del proyecto del que deriva esta publicación. Null si no está vinculada a ningún proyecto.</summary>
    public string? ProyectoId { get; init; }
}

public class CreatePublicationCommandHandler : IRequestHandler<CreatePublicationCommand, (Result Result, string? PublicationId)>
{
    private readonly IPublicationService _service;

    public CreatePublicationCommandHandler(IPublicationService service)
    {
        _service = service;
    }

    public Task<(Result Result, string? PublicationId)> Handle(CreatePublicationCommand request, CancellationToken cancellationToken)
    {
        var req = new CreatePublicationRequest
        {
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

        return _service.CreateAsync(req, cancellationToken);
    }
}
