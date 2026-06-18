using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Authors.Queries.SearchCoauthors;

/// <summary>
/// Búsqueda unificada de candidatos a coautor:<br/>
/// - Autores existentes en la BD cuyo nombre contenga el término.<br/>
/// - Usuarios registrados sin perfil de autor todavía cuyo nombre completo contenga el término.<br/>
/// Devuelve hasta 10 resultados ordenados alfabéticamente.
/// </summary>
public record SearchCoauthorsQuery(string Q) : IRequest<List<CoauthorSearchDto>>;

/// <summary>
/// Resultado de búsqueda de coautores candidatos.
/// </summary>
/// <param name="Id">
/// Si <c>Type == "author"</c>: ID del registro Author.<br/>
/// Si <c>Type == "user"</c>: ID del usuario (se creará el Author al guardar).
/// </param>
/// <param name="Name">Nombre para mostrar.</param>
/// <param name="Type"><c>"author"</c> o <c>"user"</c>.</param>
public record CoauthorSearchDto(string Id, string Name, string Type);

public class SearchCoauthorsQueryHandler : IRequestHandler<SearchCoauthorsQuery, List<CoauthorSearchDto>>
{
    private readonly IApplicationDbContext _context;

    public SearchCoauthorsQueryHandler(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<CoauthorSearchDto>> Handle(
        SearchCoauthorsQuery request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Q) || request.Q.Trim().Length < 2)
            return [];

        var term = request.Q.Trim().ToLower();

        // Autores existentes que coinciden con el término
        var authors = await _context.Authors
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(term))
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new CoauthorSearchDto(a.Id, a.Name, "author"))
            .ToListAsync(cancellationToken);

        // IDs de usuarios que YA tienen un perfil de autor (para excluirlos de la búsqueda de usuarios)
        var userIdsWithAuthor = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId != null)
            .Select(a => a.UserId!)
            .ToListAsync(cancellationToken);

        // Usuarios sin perfil de autor cuyo nombre completo coincide con el término
        var users = await _context.Users
            .AsNoTracking()
            .Where(u =>
                !userIdsWithAuthor.Contains(u.Id) &&
                (u.UserName.ToLower().Contains(term) ||
                 u.UserLastName1.ToLower().Contains(term) ||
                 (u.UserLastName2 != null && u.UserLastName2.ToLower().Contains(term))))
            .OrderBy(u => u.UserName)
            .Take(10)
            .Select(u => new CoauthorSearchDto(
                u.Id,
                (u.UserName + " " + u.UserLastName1 +
                    (u.UserLastName2 != null ? " " + u.UserLastName2 : "")).Trim(),
                "user"))
            .ToListAsync(cancellationToken);

        return [.. authors, .. users];
    }
}
