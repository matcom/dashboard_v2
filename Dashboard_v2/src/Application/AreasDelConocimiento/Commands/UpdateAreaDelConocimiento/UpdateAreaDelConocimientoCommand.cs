using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.AreasDelConocimiento.Commands.UpdateAreaDelConocimiento;

public record UpdateAreaDelConocimientoCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    public IList<string> LineasDeInvestigacionIds { get; init; } = [];
}

public class UpdateAreaDelConocimientoCommandHandler : IRequestHandler<UpdateAreaDelConocimientoCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateAreaDelConocimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateAreaDelConocimientoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure(["El nombre es obligatorio."]);

        var entity = await _context.AreasDelConocimiento
            .Include(a => a.LineasDeInvestigacion)
            .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Área del conocimiento no encontrada."]);

        entity.Nombre = request.Nombre.Trim();
        entity.Descripcion = request.Descripcion?.Trim();

        var newLineas = await _context.LineasDeInvestigacion
            .Where(l => request.LineasDeInvestigacionIds.Contains(l.Id))
            .ToListAsync(cancellationToken);
        entity.LineasDeInvestigacion.Clear();
        foreach (var linea in newLineas)
            entity.LineasDeInvestigacion.Add(linea);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
