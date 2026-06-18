using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Events.Commands.CreateEvent;

public record CreateEventCommand : IRequest<(Result Result, int? EventId)>
{
    public string Name { get; init; } = default!;
    public int CountryId { get; init; }
    /// <summary>Id del tipo de evento (tabla EventTypes).</summary>
    public int EventTypeId { get; init; }
    public List<string> Institutions { get; init; } = [];
}

public class CreateEventCommandHandler
    : IRequestHandler<CreateEventCommand, (Result Result, int? EventId)>
{
    private readonly IApplicationDbContext _context;

    public CreateEventCommandHandler(IApplicationDbContext context)
        => _context = context;

    public async Task<(Result Result, int? EventId)> Handle(
        CreateEventCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return (Result.Failure(["El nombre del evento es obligatorio."]), null);

        if (!await _context.Countries.AnyAsync(c => c.Id == request.CountryId, cancellationToken))
            return (Result.Failure(["País no válido."]), null);

        if (!await _context.EventTypes.AnyAsync(t => t.Id == request.EventTypeId, cancellationToken))
            return (Result.Failure(["Tipo de evento no válido."]), null);

        var ev = new Event
        {
            Name = request.Name.Trim(),
            CountryId = request.CountryId,
            EventTypeId = request.EventTypeId,
            Institutions = request.Institutions
                .Where(i => !string.IsNullOrWhiteSpace(i))
                .Select(i => i.Trim())
                .ToList(),
        };

        _context.Events.Add(ev);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), ev.Id);
    }
}
