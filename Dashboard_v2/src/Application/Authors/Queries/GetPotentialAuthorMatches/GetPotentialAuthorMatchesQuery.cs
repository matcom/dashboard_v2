using Dashboard_v2.Application.Common.Interfaces;

namespace Dashboard_v2.Application.Authors.Queries.GetPotentialAuthorMatches;

/// <summary>
/// Busca autores sin cuenta vinculada cuyo nombre coincida con el del usuario actual.<br/>
/// Si ya tiene un perfil de autor, devuelve listas vacías.<br/>
/// <para>
/// <b>Coincidencia exacta</b>: el nombre del autor es idéntico (case-insensitive) al nombre
/// canónico del usuario (<c>Nombre Apellido1 [Apellido2]</c>).<br/>
/// <b>Coincidencia aproximada</b>: el nombre del autor contiene tanto el nombre como el primer
/// apellido del usuario (ILIKE), pero no es exacto.
/// </para>
/// </summary>
public record GetPotentialAuthorMatchesQuery : IRequest<PotentialAuthorMatchesDto>;

public record PotentialAuthorMatchDto(string Id, string Name);

public record PotentialAuthorMatchesDto(
    IReadOnlyList<PotentialAuthorMatchDto> ExactMatches,
    IReadOnlyList<PotentialAuthorMatchDto> FuzzyMatches);

public class GetPotentialAuthorMatchesQueryHandler
    : IRequestHandler<GetPotentialAuthorMatchesQuery, PotentialAuthorMatchesDto>
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public GetPotentialAuthorMatchesQueryHandler(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PotentialAuthorMatchesDto> Handle(
        GetPotentialAuthorMatchesQuery request, CancellationToken cancellationToken)
    {
        // Si ya tiene perfil de autor vinculado, no hay nada que sugerir
        var alreadyLinked = await _context.Authors
            .AnyAsync(a => a.UserId == _currentUser.Id, cancellationToken);

        if (alreadyLinked)
            return new PotentialAuthorMatchesDto([], []);

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, cancellationToken);

        if (user == null)
            return new PotentialAuthorMatchesDto([], []);

        // Nombre canónico: "Nombre Apellido1" o "Nombre Apellido1 Apellido2"
        var canonicalName = string.IsNullOrEmpty(user.UserLastName2)
            ? $"{user.UserName} {user.UserLastName1}"
            : $"{user.UserName} {user.UserLastName1} {user.UserLastName2}";

        // Candidatos: autores sin UserId cuyo nombre tenga alguna similaridad
        // Se filtra en BD con lower().Contains() → traducido a ILIKE por Npgsql
        var firstName = user.UserName;
        var lastName1 = user.UserLastName1;
        var firstNameLower = firstName.ToLower();
        var lastName1Lower = lastName1.ToLower();

        var candidates = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId == null
                && a.Name.ToLower().Contains(lastName1Lower)
                && a.Name.ToLower().Contains(firstNameLower))
            .Select(a => new { a.Id, a.Name })
            .ToListAsync(cancellationToken);

        var exact = candidates
            .Where(a => string.Equals(a.Name.Trim(), canonicalName.Trim(), StringComparison.OrdinalIgnoreCase))
            .Select(a => new PotentialAuthorMatchDto(a.Id, a.Name))
            .ToList();

        var exactIds = exact.Select(e => e.Id).ToHashSet();

        var fuzzy = candidates
            .Where(a => !exactIds.Contains(a.Id))
            .Select(a => new PotentialAuthorMatchDto(a.Id, a.Name))
            .ToList();

        return new PotentialAuthorMatchesDto(exact, fuzzy);
    }
}
