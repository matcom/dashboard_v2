using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Events.Commands.DeletePresentation;

public record DeletePresentationCommand(int Id) : IRequest<Result>;

public class DeletePresentationCommandHandler : IRequestHandler<DeletePresentationCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public DeletePresentationCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> Handle(DeletePresentationCommand request, CancellationToken cancellationToken)
    {
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
            return Result.Failure(["No tienes permiso para eliminar esta presentación."]);

        _context.Presentations.Remove(presentation);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
