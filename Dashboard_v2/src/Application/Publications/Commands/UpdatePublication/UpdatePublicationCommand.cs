using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Commands.UpdatePublication;

public record UpdatePublicationCommand : IRequest
{
    public int Id { get; init; }
    public string Title { get; init; } = default!;
    public string? AuthorRelation { get; init; }
    public DateOnly? PublicationDate { get; init; }
    public int PublicationTypeId { get; init; }

    // Especialización Revista
    public bool IsJournal { get; init; }
    public string? JournalDatabase { get; init; }
    public string? JournalGroupName { get; init; }
    public string? JournalQuartile { get; init; }

    // Especialización Publicación Indexada
    public bool IsIndexedPublication { get; init; }
    public string? IndexName { get; init; }
}

public class UpdatePublicationCommandHandler : IRequestHandler<UpdatePublicationCommand>
{
    private readonly IApplicationDbContext _context;

    public UpdatePublicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(UpdatePublicationCommand request, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .Include(p => p.Journal)
            .Include(p => p.IndexedPublication)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Publication), request.Id.ToString());

        publication.Title = request.Title;
        publication.AuthorRelation = request.AuthorRelation;
        publication.PublicationDate = request.PublicationDate;
        publication.PublicationTypeId = request.PublicationTypeId;

        // Actualizar / Crear / Eliminar especialización Revista
        if (request.IsJournal)
        {
            if (publication.Journal != null)
            {
                publication.Journal.Database = request.JournalDatabase;
                publication.Journal.GroupName = request.JournalGroupName;
                publication.Journal.Quartile = request.JournalQuartile;
            }
            else
            {
                _context.Journals.Add(new Journal
                {
                    PublicationId = publication.Id,
                    Database = request.JournalDatabase,
                    GroupName = request.JournalGroupName,
                    Quartile = request.JournalQuartile
                });
            }
        }
        else if (publication.Journal != null)
        {
            _context.Journals.Remove(publication.Journal);
        }

        // Actualizar / Crear / Eliminar especialización Publicación Indexada
        if (request.IsIndexedPublication)
        {
            if (publication.IndexedPublication != null)
            {
                publication.IndexedPublication.IndexName = request.IndexName;
            }
            else
            {
                _context.IndexedPublications.Add(new IndexedPublication
                {
                    PublicationId = publication.Id,
                    IndexName = request.IndexName
                });
            }
        }
        else if (publication.IndexedPublication != null)
        {
            _context.IndexedPublications.Remove(publication.IndexedPublication);
        }

        await _context.SaveChangesAsync(cancellationToken);
    }
}
