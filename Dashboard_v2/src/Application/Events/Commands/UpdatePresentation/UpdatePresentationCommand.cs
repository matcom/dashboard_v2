using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Events.Commands.UpdatePresentation;

public record UpdatePresentationCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public int EventId { get; init; }
    public List<string> CoauthorIds { get; init; } = [];
    public List<string> CoauthorNames { get; init; } = [];
}

public class UpdatePresentationCommandHandler : IRequestHandler<UpdatePresentationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public UpdatePresentationCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(UpdatePresentationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure(["El nombre de la presentación es obligatorio."]);

        var authorId = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId == _currentUser.Id)
            .Select(a => a.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (authorId is null)
            return Result.Failure(["No tienes un perfil de autor."]);

        var presentation = await _context.Presentations
            .Include(p => p.AuthorPresentations)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (presentation is null)
            return Result.Failure(["Presentación no encontrada."]);

        if (!presentation.AuthorPresentations.Any(ap => ap.AuthorId == authorId))
            return Result.Failure(["No tienes permiso para modificar esta presentación."]);

        var eventExists = await _context.Events.AnyAsync(e => e.Id == request.EventId, cancellationToken);
        if (!eventExists)
            return Result.Failure(["El evento seleccionado no existe."]);

        presentation.Name = request.Name.Trim();
        presentation.EventId = request.EventId;

        // Replace all non-creator authors
        var toRemove = presentation.AuthorPresentations
            .Where(ap => ap.AuthorId != authorId)
            .ToList();
        foreach (var link in toRemove)
            _context.AuthorPresentations.Remove(link);

        foreach (var id in request.CoauthorIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (id != authorId && await _context.Authors.AnyAsync(a => a.Id == id, cancellationToken))
                presentation.AuthorPresentations.Add(new AuthorPresentation { AuthorId = id });
        }

        foreach (var name in request.CoauthorNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var coauthor = new Author { Name = name.Trim() };
            _context.Authors.Add(coauthor);
            await _context.SaveChangesAsync(cancellationToken);
            presentation.AuthorPresentations.Add(new AuthorPresentation { AuthorId = coauthor.Id });
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
