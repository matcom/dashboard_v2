using Dashboard_v2.Application.Common.Exceptions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Security;
using Dashboard_v2.Domain.Constants;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Publications.Commands.DeletePublication;

[Authorize]
public record DeletePublicationCommand(int Id) : IRequest;

public class DeletePublicationCommandHandler : IRequestHandler<DeletePublicationCommand>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;
    private readonly IPermissionService _permissionService;

    public DeletePublicationCommandHandler(
        IApplicationDbContext context,
        IUser currentUser,
        IPermissionService permissionService)
    {
        _context = context;
        _currentUser = currentUser;
        _permissionService = permissionService;
    }

    public async Task Handle(DeletePublicationCommand request, CancellationToken cancellationToken)
    {
        var publication = await _context.Publications
            .Include(p => p.Resource)
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new NotFoundException(nameof(Publication), request.Id.ToString());

        // Verificar permiso: delete_any de sistema, O ser dueño del recurso, O grant "delete"/"admin"
        var canDelete = await _permissionService.HasSystemPermissionAsync(_currentUser.Id!, SystemPermissions.DeleteAnyPublication, cancellationToken)
                        || await _permissionService.HasPermissionAsync(_currentUser.Id!, publication.ResourceId, "delete", cancellationToken)
                        || await _permissionService.HasPermissionAsync(_currentUser.Id!, publication.ResourceId, "admin", cancellationToken);
        if (!canDelete)
            throw new ForbiddenAccessException();

        // Eliminar el Resource (cascade borra Publication, Journal, IndexedPublication, ResourceGrants)
        _context.Resources.Remove(publication.Resource);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
