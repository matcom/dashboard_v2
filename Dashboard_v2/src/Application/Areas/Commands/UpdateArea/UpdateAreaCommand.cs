using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.Areas.Commands.UpdateArea;

public record UpdateAreaCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    /// <summary>
    /// Id de la Universidad a la que pertenece el Área.
    /// Pasar null desvincula el Área de cualquier Universidad.
    /// </summary>
    public string? UniversidadId { get; init; }
}

public class UpdateAreaCommandHandler : IRequestHandler<UpdateAreaCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAreaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAreaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure(["El nombre es obligatorio."]);

        var area = await _context.Areas
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (area is null)
            return Result.Failure(["Área no encontrada."]);

        if (request.UniversidadId is not null &&
            !await _context.Universidades.AnyAsync(u => u.Id == request.UniversidadId, cancellationToken))
            return Result.Failure(["La universidad indicada no existe."]);

        area.Nombre = request.Nombre.Trim();
        area.Descripcion = request.Descripcion?.Trim();
        area.UniversidadId = request.UniversidadId;

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
