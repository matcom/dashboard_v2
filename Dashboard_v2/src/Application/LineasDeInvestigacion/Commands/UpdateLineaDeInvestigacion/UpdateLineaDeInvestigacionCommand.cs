using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;

namespace Dashboard_v2.Application.LineasDeInvestigacion.Commands.UpdateLineaDeInvestigacion;

public record UpdateLineaDeInvestigacionCommand : IRequest<Result>
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    public IList<string> AreasDelConocimientoIds { get; init; } = [];
}

public class UpdateLineaDeInvestigacionCommandHandler : IRequestHandler<UpdateLineaDeInvestigacionCommand, Result>
{
    private readonly IApplicationDbContext _context;

    public UpdateLineaDeInvestigacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Result> Handle(UpdateLineaDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return Result.Failure(["El nombre es obligatorio."]);

        var entity = await _context.LineasDeInvestigacion
            .Include(l => l.AreasDelConocimiento)
            .FirstOrDefaultAsync(l => l.Id == request.Id, cancellationToken);

        if (entity is null)
            return Result.Failure(["Línea de investigación no encontrada."]);

        entity.Nombre = request.Nombre.Trim();
        entity.Descripcion = request.Descripcion?.Trim();

        var newAreas = await _context.AreasDelConocimiento
            .Where(a => request.AreasDelConocimientoIds.Contains(a.Id))
            .ToListAsync(cancellationToken);
        entity.AreasDelConocimiento.Clear();
        foreach (var area in newAreas)
            entity.AreasDelConocimiento.Add(area);

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
