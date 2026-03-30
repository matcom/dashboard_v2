using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Events.Commands.DeleteEvent;

public record DeleteEventCommand(int Id) : IRequest<Result>;

public class DeleteEventCommandHandler : IRequestHandler<DeleteEventCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public DeleteEventCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result> Handle(DeleteEventCommand request, CancellationToken cancellationToken)
    {
        var ev = await _context.Events.FindAsync([request.Id], cancellationToken);
        if (ev is null)
            return Result.Failure(["Evento no encontrado."]);

        var hasPresentations = await _context.Presentations
            .AnyAsync(p => p.EventId == request.Id, cancellationToken);

        if (hasPresentations)
            return Result.Failure(["No se puede eliminar un evento que tiene presentaciones registradas."]);

        _context.Events.Remove(ev);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
