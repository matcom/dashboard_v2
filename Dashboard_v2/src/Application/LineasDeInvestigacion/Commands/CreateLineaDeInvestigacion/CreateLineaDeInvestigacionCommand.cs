using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.LineasDeInvestigacion.Commands.CreateLineaDeInvestigacion;

public record CreateLineaDeInvestigacionCommand : IRequest<(Result Result, string? Id)>
{
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    public IList<string> AreasDelConocimientoIds { get; init; } = [];
}

public class CreateLineaDeInvestigacionCommandHandler : IRequestHandler<CreateLineaDeInvestigacionCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateLineaDeInvestigacionCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Result Result, string? Id)> Handle(CreateLineaDeInvestigacionCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (Result.Failure(["El nombre es obligatorio."]), null);

        var entity = new LineaDeInvestigacion
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };

        if (request.AreasDelConocimientoIds.Count > 0)
        {
            var areas = await _context.AreasDelConocimiento
                .Where(a => request.AreasDelConocimientoIds.Contains(a.Id))
                .ToListAsync(cancellationToken);
            entity.AreasDelConocimiento = areas;
        }

        _context.LineasDeInvestigacion.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), entity.Id);
    }
}
