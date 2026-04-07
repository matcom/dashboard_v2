using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Areas.Queries.GetAreas;

public record GetAreasQuery : IRequest<List<AreaDto>>;

public class GetAreasQueryHandler : IRequestHandler<GetAreasQuery, List<AreaDto>>
{
    private readonly IApplicationDbContext _context;

    public GetAreasQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AreaDto>> Handle(GetAreasQuery request, CancellationToken cancellationToken)
    {
        return await _context.Areas
            .OrderBy(a => a.Nombre)
            .Select(a => new AreaDto
            {
                Id = a.Id,
                Nombre = a.Nombre,
                Descripcion = a.Descripcion,
                UniversidadId = a.UniversidadId,
                UniversidadNombre = a.Universidad != null ? a.Universidad.Nombre : null,
                AreasDelConocimientoIds = a.AreasDelConocimiento.Select(ac => ac.Id).ToList()
            })
            .ToListAsync(cancellationToken);
    }
}
