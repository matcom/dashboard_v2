using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Events.Commands.UpdateEvent;

public record UpdateEventCommand : IRequest<Result>
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public int CountryId { get; init; }
    public EventTypeEnum EventType { get; init; }
    public List<string> Institutions { get; init; } = [];
}

public class UpdateEventCommandHandler : IRequestHandler<UpdateEventCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateEventCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<Result> Handle(UpdateEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return Result.Failure(["El nombre del evento es obligatorio."]);

        var ev = await _context.Events.FindAsync([request.Id], cancellationToken);
        if (ev is null)
            return Result.Failure(["Evento no encontrado."]);

        if (!await _context.Countries.AnyAsync(c => c.Id == request.CountryId, cancellationToken))
            return Result.Failure(["País no válido."]);

        if (!Enum.IsDefined(typeof(EventTypeEnum), request.EventType))
            return Result.Failure(["Tipo de evento no válido."]);

        ev.Name = request.Name.Trim();
        ev.CountryId = request.CountryId;
        ev.EventType = request.EventType;
        ev.Institutions = request.Institutions
            .Where(i => !string.IsNullOrWhiteSpace(i))
            .Select(i => i.Trim())
            .ToList();

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }
}
