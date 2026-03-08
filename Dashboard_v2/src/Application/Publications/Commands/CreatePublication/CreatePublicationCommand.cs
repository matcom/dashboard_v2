using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Publications.Commands.CreatePublication;

[Authorize(SystemPermission = SystemPermissions.CreatePublications)]
public record CreatePublicationCommand : IRequest<int>
{
    public string Title { get; init; } = default!;
    public string? AuthorRelation { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public int PublicationTypeId { get; init; }

    // Especialización opcional: Revista
    public bool IsJournal { get; init; }
    public string? JournalDatabase { get; init; }
    public string? JournalGroupName { get; init; }
    public string? JournalQuartile { get; init; }

    // Especialización opcional: Publicación Indexada
    public bool IsIndexedPublication { get; init; }
    public string? IndexName { get; init; }
}

public class CreatePublicationCommandHandler : IRequestHandler<CreatePublicationCommand, int>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public CreatePublicationCommandHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<int> Handle(CreatePublicationCommand request, CancellationToken cancellationToken)
    {
        // 1. Crear el Resource para el sistema de permisos
        var resource = new Resource
        {
            Type = "Publication",
            OwnerId = _currentUser.Id!
        };
        _context.Resources.Add(resource);
        await _context.SaveChangesAsync(cancellationToken);

        // 2. Crear la publicación vinculada al Resource
        var publication = new Publication
        {
            ResourceId = resource.Id,
            Title = request.Title,
            AuthorRelation = request.AuthorRelation,
            PublicationDate = request.PublicationDate,
            PublicationTypeId = request.PublicationTypeId
        };
        _context.Publications.Add(publication);
        await _context.SaveChangesAsync(cancellationToken);

        // 3. Especialización: Revista
        if (request.IsJournal)
        {
            _context.Journals.Add(new Journal
            {
                PublicationId = publication.Id,
                Database = request.JournalDatabase,
                GroupName = request.JournalGroupName,
                Quartile = request.JournalQuartile
            });
        }

        // 4. Especialización: Publicación Indexada
        if (request.IsIndexedPublication)
        {
            _context.IndexedPublications.Add(new IndexedPublication
            {
                PublicationId = publication.Id,
                IndexName = request.IndexName
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        return publication.Id;
    }
}
