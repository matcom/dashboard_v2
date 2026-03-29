using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

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
    public string PublicationTypeId { get; init; } = default!;
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
}

public class UpdatePublicationCommandHandler : IRequestHandler<UpdatePublicationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IAuthorCleanupService _authorCleanup;

    public UpdatePublicationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IAuthorCleanupService authorCleanup)
    {
        _context = context;
        _currentUser = currentUser;
        _authorCleanup = authorCleanup;
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
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (publication == null)
            return Result.Failure(["Publicación no encontrada."]);

        // Validar que el nuevo tipo existe
        var typeExists = await _context.PublicationTypes
            .AnyAsync(pt => pt.Id == request.PublicationTypeId, cancellationToken);
        if (!typeExists)
            return Result.Failure(["Tipo de publicación no encontrado."]);

        publication.Title = request.Title.Trim();
        publication.PublicationData = request.PublicationData;
        publication.PublicationTypeId = request.PublicationTypeId;
        publication.UrlDoi = string.IsNullOrWhiteSpace(request.UrlDoi) ? null : request.UrlDoi.Trim();

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

        // Agregar coautores existentes por ID
        foreach (var authorId in request.AdditionalAuthorIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (authorId != currentAuthor.Id && await _context.Authors.AnyAsync(a => a.Id == authorId, cancellationToken))
                publication.AuthorPublications.Add(new AuthorPublication { AuthorId = authorId });
        }

        // Agregar coautores nuevos por nombre
        foreach (var name in request.AdditionalAuthorNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            publication.AuthorPublications.Add(new AuthorPublication
            {
                Author = new Author { Name = name.Trim() }
            });
        }

        // Agregar coautores referenciados como usuarios (find-or-create author vinculado)
        foreach (var userId in request.AdditionalUserIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (userId == _currentUser.Id) continue;

            var coAuthor = await _context.Authors
                .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

            if (coAuthor == null)
            {
                var coUser = await _context.Users
                    .AsNoTracking()
                    .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);
                if (coUser == null) continue;

                coAuthor = new Author
                {
                    Name = $"{coUser.UserName} {coUser.UserLastName1}{(string.IsNullOrEmpty(coUser.UserLastName2) ? string.Empty : " " + coUser.UserLastName2)}".Trim(),
                    UserId = coUser.Id
                };
                _context.Authors.Add(coAuthor);
                await _context.SaveChangesAsync(cancellationToken);
            }

            if (publication.AuthorPublications.All(ap => ap.AuthorId != coAuthor.Id))
                publication.AuthorPublications.Add(new AuthorPublication { AuthorId = coAuthor.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);

        // Eliminar autores que ya no tienen ninguna referencia y no están vinculados a un usuario
        await _authorCleanup.CleanupIfOrphanedAsync(removedAuthorIds, cancellationToken);

        return Result.Success();
    }
}
