using Dashboard_v2.Application.AreasDelConocimiento;
using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.AreasDelConocimiento.Queries.GetAreasDelConocimiento;

public record GetAreasDelConocimientoQuery : IRequest<List<AreaDelConocimientoDto>>;

public class GetAreasDelConocimientoQueryHandler : IRequestHandler<GetAreasDelConocimientoQuery, List<AreaDelConocimientoDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAreasDelConocimientoQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AreaDelConocimientoDto>> Handle(GetAreasDelConocimientoQuery request, CancellationToken cancellationToken)
    {
        return await _context.AreasDelConocimiento
            .OrderBy(a => a.Nombre)
            .Select(a => new AreaDelConocimientoDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                LineasDeInvestigacionIds = a.LineasDeInvestigacion.Select(l => l.Id).ToList(),
            })
            .ToListAsync(cancellationToken);
    }
}
