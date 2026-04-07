using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.LineasDeInvestigacion;

namespace Dashboard_v2.Application.LineasDeInvestigacion.Queries.GetLineasDeInvestigacion;

public record GetLineasDeInvestigacionQuery : IRequest<List<LineaDeInvestigacionDto>>;

public class GetLineasDeInvestigacionQueryHandler : IRequestHandler<GetLineasDeInvestigacionQuery, List<LineaDeInvestigacionDto>>
{
    private readonly IApplicationDbContext _context;

    public GetLineasDeInvestigacionQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<LineaDeInvestigacionDto>> Handle(GetLineasDeInvestigacionQuery request, CancellationToken cancellationToken)
    {
        return await _context.LineasDeInvestigacion
            .OrderBy(l => l.Nombre)
            .Select(l => new LineaDeInvestigacionDto
            {
                Id = l.Id,
                Nombre = l.Nombre,
                Descripcion = l.Descripcion,
                AreasDelConocimientoIds = l.AreasDelConocimiento.Select(a => a.Id).ToList(),
                AreasDelConocimientoNombres = l.AreasDelConocimiento.Select(a => a.Nombre).ToList(),
            })
            .ToListAsync(cancellationToken);
    }
}
