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
    public string? JournalName { get; init; }
    public string? DataBase { get; init; }
    public int? Group { get; init; }
    public Cuartil? Cuartil { get; init; }
}

public class UpdatePublicationCommandHandler : IRequestHandler<UpdatePublicationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IAuthorCleanupService _authorCleanup;
    private readonly IAuthorResolutionService _authorResolution;
    private readonly IPublicationSpecializationService _specialization;

    public UpdatePublicationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IAuthorCleanupService authorCleanup,
        IAuthorResolutionService authorResolution,
        IPublicationSpecializationService specialization)
    {
        _context = context;
        _currentUser = currentUser;
        _authorCleanup = authorCleanup;
        _authorResolution = authorResolution;
        _specialization = specialization;
    }

    public async Task<Result> Handle(UpdatePublicationCommand request, CancellationToken cancellationToken)
    {
        // Verificar que el usuario tiene perfil de autor y es autor de esta publicación
        var currentAuthor = await _context.Authors
            .FirstOrDefaultAsync(a => a.UserId == _currentUser.Id, cancellationToken);

        if (currentAuthor == null)
            return Result.Failure(["Publicación no encontrada o no tienes permiso para editarla."]);

        var isAuthor = await _context.AuthorPublications
            .AnyAsync(ap => ap.PublicationId == request.Id && ap.AuthorId == currentAuthor.Id, cancellationToken);

        if (!isAuthor)
            return Result.Failure(["Publicación no encontrada o no tienes permiso para editarla."]);

        var publication = await _context.Publications
            .Include(p => p.AuthorPublications)
            .Include(p => p.JournalPublication)
                .ThenInclude(jp => jp!.JournalGroup1Publication)
            .Include(p => p.IndexedPublication)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (publication == null)
            return Result.Failure(["Publicación no encontrada."]);

        // Validar tipo y campos de especialización (delegado al servicio)
        if (!Enum.IsDefined(typeof(PublicationType), request.PublicationType))
            return Result.Failure(["Tipo de publicación no válido."]);

        var specializationData = new PublicationSpecializationData(
            request.PublicationType,
            request.JournalName,
            request.DataBase,
            request.Group,
            request.Cuartil,
            request.Index);

        var validationError = _specialization.Validate(specializationData);
        if (validationError != null)
            return Result.Failure([validationError]);

        // Actualizar campos base
        publication.Title = request.Title.Trim();
        publication.PublicationData = request.PublicationData;
        publication.PublicationType = request.PublicationType;
        publication.UrlDoi = string.IsNullOrWhiteSpace(request.UrlDoi) ? null : request.UrlDoi.Trim();

        // Actualizar especialización (delegado al servicio)
        await _specialization.ApplySpecializationUpdateAsync(publication, specializationData, cancellationToken);

        // Reemplazar coautores: conservar solo el autor actual y añadir los nuevos
        var removedAuthorIds = publication.AuthorPublications
            .Where(ap => ap.AuthorId != currentAuthor.Id)
            .Select(ap => ap.AuthorId)
            .ToList();
        foreach (var authorIdToRemove in removedAuthorIds)
        {
            var ap = publication.AuthorPublications.First(x => x.AuthorId == authorIdToRemove);
            publication.AuthorPublications.Remove(ap);
        }

        // Agregar coautores adicionales (delegado al servicio)
        var coauthors = await _authorResolution.ResolveCoauthorsAsync(
            currentAuthor.Id,
            request.AdditionalAuthorIds,
            request.AdditionalAuthorNames,
            request.AdditionalUserIds,
            cancellationToken);
        foreach (var ap in coauthors)
            publication.AuthorPublications.Add(ap);

        await _context.SaveChangesAsync(cancellationToken);

        // Limpiar autores huérfanos que ya no tienen ninguna referencia
        await _authorCleanup.CleanupIfOrphanedAsync(removedAuthorIds, cancellationToken);

        return Result.Success();
    }
}
