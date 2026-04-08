using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Authors.Queries.SearchAuthors;

/// <summary>
/// Busca autores cuyo nombre contenga el término indicado (case-insensitive).<br/>
/// Devuelve hasta 10 resultados ordenados alfabéticamente.
/// Requiere al menos 2 caracteres de búsqueda.
/// </summary>
public record SearchAuthorsQuery(string Q) : IRequest<List<AuthorSearchDto>>;

public record AuthorSearchDto(string Id, string Name);

public class SearchAuthorsQueryHandler : IRequestHandler<SearchAuthorsQuery, List<AuthorSearchDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchAuthorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<AuthorSearchDto>> Handle(SearchAuthorsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Q) || request.Q.Trim().Length < 2)
            return [];

        var term = request.Q.Trim().ToLower();

        return await _context.Authors
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(term))
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new AuthorSearchDto(a.Id, a.Name))
            .ToListAsync(cancellationToken);
    }
}
