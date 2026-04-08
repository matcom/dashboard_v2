using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.AreasDelConocimiento.Commands.CreateAreaDelConocimiento;

public record CreateAreaDelConocimientoCommand : IRequest<(Result Result, string? Id)>
{
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    public IList<string> LineasDeInvestigacionIds { get; init; } = [];
}

public class CreateAreaDelConocimientoCommandHandler : IRequestHandler<CreateAreaDelConocimientoCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateAreaDelConocimientoCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Result Result, string? Id)> Handle(CreateAreaDelConocimientoCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (Result.Failure(["El nombre es obligatorio."]), null);

        var entity = new AreaDelConocimiento
        {
            Id = Guid.NewGuid().ToString(),
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
        };

        if (request.LineasDeInvestigacionIds.Count > 0)
        {
            var lineas = await _context.LineasDeInvestigacion
                .Where(l => request.LineasDeInvestigacionIds.Contains(l.Id))
                .ToListAsync(cancellationToken);
            entity.LineasDeInvestigacion = lineas;
        }

        _context.AreasDelConocimiento.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), entity.Id);
    }
}
