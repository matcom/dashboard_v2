using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Events.Commands.CreatePresentation;

public record CreatePresentationCommand : IRequest<(Result Result, int? PresentationId)>
{
    public string Name { get; init; } = default!;
    public int EventId { get; init; }
    /// <summary>Ids de autores adicionales (Author.Id).</summary>
    public List<string> CoauthorIds { get; init; } = [];
    /// <summary>Nombres libres de autores sin cuenta.</summary>
    public List<string> CoauthorNames { get; init; } = [];
}

public class CreatePresentationCommandHandler
    : IRequestHandler<CreatePresentationCommand, (Result Result, int? PresentationId)>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IAuthorResolutionService _authorResolution;

    public CreatePresentationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IAuthorResolutionService authorResolution)
    {
        _context = context;
        _currentUser = currentUser;
        _authorResolution = authorResolution;
    }

    public async Task<(Result Result, int? PresentationId)> Handle(
        CreatePresentationCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (Result.Failure(["El nombre de la presentación es obligatorio."]), null);

        var eventExists = await _context.Events
            .AnyAsync(e => e.Id == request.EventId, cancellationToken);
        if (!eventExists)
            return (Result.Failure(["El evento seleccionado no existe."]), null);

        // Obtener o crear el perfil de autor del usuario actual (delegado al servicio)
        var author = await _authorResolution.ResolveOrCreateByUserIdAsync(_currentUser.Id!, cancellationToken);
        if (author is null)
            return (Result.Failure(["Usuario no encontrado."]), null);

        var presentation = new Presentation
        {
            Name = request.Name.Trim(),
            EventId = request.EventId,
            AuthorPresentations = [new AuthorPresentation { AuthorId = author.Id }],
        };

        // Additional authors by existing Author.Id
        foreach (var id in request.CoauthorIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (id != author.Id && await _context.Authors.AnyAsync(a => a.Id == id, cancellationToken))
                presentation.AuthorPresentations.Add(new AuthorPresentation { AuthorId = id });
        }

        // Additional authors by free-text name (create new Author record)
        foreach (var name in request.CoauthorNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            var coauthor = new Author { Name = name.Trim() };
            _context.Authors.Add(coauthor);
            await _context.SaveChangesAsync(cancellationToken);
            presentation.AuthorPresentations.Add(new AuthorPresentation { AuthorId = coauthor.Id });
        }

        _context.Presentations.Add(presentation);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), presentation.Id);
    }
}
