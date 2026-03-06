using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Commands.DeletePublication;

public record DeletePublicationCommand(int Id) : IRequest;

public class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand>
{
    private readonly IApplicationDbContext _context;

    public DeletePublicationCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .Include(p => p.Resource)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Publication), request.Id.ToString());

        // Eliminar el Resource (cascade borra Publication, Journal, IndexedPublication, ResourceGrants)
        _context.Resources.Remove(publication.Resource);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
