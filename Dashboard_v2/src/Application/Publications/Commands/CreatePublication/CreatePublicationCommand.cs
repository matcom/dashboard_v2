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

    // ── Revista (tipo Diario) ─────────────────────────────────────────────────────────────────
    /// <summary>Nombre de la revista. Obligatorio para tipo Diario.</summary>
    public string? JournalName { get; init; }
    public string? JournalISSN { get; init; }
    public string? JournalEISSN { get; init; }

    // ── Base de datos ─────────────────────────────────────────────────────────────────────────
    public string? DatabaseName { get; init; }
    public string? DatabaseUrl { get; init; }

    /// <summary>Grupo de la revista (1–4). Obligatorio para tipo Diario.</summary>
    public int? Group { get; init; }
    /// <summary>Cuartil Scimago. Obligatorio para tipo Diario cuando la base de datos es Scopus.</summary>
    public Cuartil? Cuartil { get; init; }
}

public class CreatePublicationCommandHandler : IRequestHandler<CreatePublicationCommand, (Result Result, string? PublicationId)>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IAuthorResolutionService _authorResolution;
    private readonly IPublicationSpecializationService _specialization;

    public CreatePublicationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IAuthorResolutionService authorResolution,
        IPublicationSpecializationService specialization)
    {
        _context = context;
        _currentUser = currentUser;
        _authorResolution = authorResolution;
        _specialization = specialization;
    }

    public async Task<(Result Result, string? PublicationId)> Handle(
        CreatePublicationCommand request, CancellationToken cancellationToken)
    {
        // Validar que el tipo de publicación es válido
        if (!Enum.IsDefined(typeof(PublicationType), request.PublicationType))
            return (Result.Failure(["Tipo de publicación no válido."]), null);

        // Validar campos de especialización (delegado al servicio)
        var specializationData = new PublicationSpecializationData(
            request.PublicationType,
            request.JournalName,
            request.JournalISSN,
            request.JournalEISSN,
            request.DatabaseName,
            request.DatabaseUrl,
            request.Group,
            request.Cuartil,
            request.Index);

        var validationError = _specialization.Validate(specializationData);
        if (validationError != null)
            return (Result.Failure([validationError]), null);

        // Obtener o crear el perfil de autor del usuario actual
        var author = await _authorResolution.ResolveOrCreateByUserIdAsync(_currentUser.Id!, cancellationToken);
        if (author == null)
            return (Result.Failure(["Usuario no encontrado."]), null);

        // Construir la publicación con el usuario actual como primer autor
        var publication = new Publication
        {
            Title = request.Title.Trim(),
            PublicationData = request.PublicationData,
            PublicationType = request.PublicationType,
            UrlDoi = string.IsNullOrWhiteSpace(request.UrlDoi) ? null : request.UrlDoi.Trim(),
            AuthorPublications = [new AuthorPublication { AuthorId = author.Id }]
        };

        // Agregar coautores adicionales (delegado al servicio)
        var coauthors = await _authorResolution.ResolveCoauthorsAsync(
            author.Id,
            request.AdditionalAuthorIds,
            request.AdditionalAuthorNames,
            request.AdditionalUserIds,
            cancellationToken);
        foreach (var ap in coauthors)
            publication.AuthorPublications.Add(ap);

        // Vincular especialización según el tipo (delegado al servicio)
        _specialization.AttachSpecialization(publication, specializationData);

        _context.Publications.Add(publication);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), publication.Id);
    }
}
