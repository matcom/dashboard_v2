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
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IAuthorCleanupService _authorCleanup;

    public DeletePublicationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IAuthorCleanupService authorCleanup)
    {
        _context = context;
        _currentUser = currentUser;
        _authorCleanup = authorCleanup;
    }

    public async Task<Result> Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        // Verificar autoría antes de buscar y eliminar
        var isAuthor = await _context.AuthorPublications
            .AnyAsync(ap =>
                ap.PublicationId == request.Id &&
                ap.Author.UserId == _currentUser.Id,
                cancellationToken);

        if (!isAuthor)
            return Result.Failure(["Publicación no encontrada o no tienes permiso para eliminarla."]);

        var publication = await _context.Publications
            .Include(p => p.AuthorPublications)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (publication == null)
            return Result.Failure(["Publicación no encontrada."]);

        // Capturar los IDs de autores antes de que el cascade los elimine
        var authorIds = publication.AuthorPublications
            .Select(ap => ap.AuthorId)
            .ToList();

        // El Cascade en AuthorPublicationConfiguration elimina las filas de AuthorPublications automáticamente
        _context.Publications.Remove(publication);
        await _context.SaveChangesAsync(cancellationToken);

        // Eliminar autores que ya no tienen ninguna referencia y no están vinculados a un usuario
        await _authorCleanup.CleanupIfOrphanedAsync(authorIds, cancellationToken);

        return Result.Success();
    }
}
