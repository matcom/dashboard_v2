using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Areas.Commands.CreateArea;

public record CreateAreaCommand : IRequest<(Result Result, string? Id)>
{
    public string Nombre { get; init; } = default!;
    public string? Descripcion { get; init; }
    /// <summary>Id de la Universidad a la que pertenece el Área. Opcional.</summary>
    public string? UniversidadId { get; init; }
    /// <summary>Ids de las Áreas del Conocimiento que investiga esta Área (relación N:N).</summary>
    public IList<string> AreasDelConocimientoIds { get; init; } = [];
}

public class CreateAreaCommandHandler : IRequestHandler<CreateAreaCommand, (Result Result, string? Id)>
{
    private readonly IApplicationDbContext _context;

    public CreateAreaCommandHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<(Result Result, string? Id)> Handle(CreateAreaCommand request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre))
            return (Result.Failure(["El nombre es obligatorio."]), null);

        if (request.UniversidadId is not null &&
            !await _context.Universidades.AnyAsync(u => u.Id == request.UniversidadId, cancellationToken))
            return (Result.Failure(["La universidad indicada no existe."]), null);

        var area = new Area
        {
            Nombre = request.Nombre.Trim(),
            Descripcion = request.Descripcion?.Trim(),
            UniversidadId = request.UniversidadId
        };

        if (request.AreasDelConocimientoIds.Count > 0)
        {
            var areasConocimiento = await _context.AreasDelConocimiento
                .Where(a => request.AreasDelConocimientoIds.Contains(a.Id))
                .ToListAsync(cancellationToken);
            area.AreasDelConocimiento = areasConocimiento;
        }

        _context.Areas.Add(area);
        await _context.SaveChangesAsync(cancellationToken);

        return (Result.Success(), area.Id);
    }
}
