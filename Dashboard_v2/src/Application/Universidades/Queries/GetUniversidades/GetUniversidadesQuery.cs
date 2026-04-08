using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Universidades.Queries.GetUniversidades;

public record GetUniversidadesQuery : IRequest<List<UniversidadDto>>;

public class GetUniversidadesQueryHandler : IRequestHandler<GetUniversidadesQuery, List<UniversidadDto>>
{
    private readonly IApplicationDbContext _context;

    public GetUniversidadesQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<UniversidadDto>> Handle(GetUniversidadesQuery request, CancellationToken cancellationToken)
    {
        return await _context.Universidades
            .OrderBy(u => u.Nombre)
            .Select(u => new UniversidadDto { Id = u.Id, Nombre = u.Nombre })
            .ToListAsync(cancellationToken);
    }
}
