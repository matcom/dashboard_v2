using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Authors;

public sealed class AuthorService : IAuthorService
{
    private readonly IApplicationDbContext _context;
    private readonly IUser _currentUser;

    public AuthorService(IApplicationDbContext context, IUser currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Result> LinkToUserAsync(string authorId, CancellationToken ct = default)
    {
        var userAlreadyLinked = await _context.Authors
            .AnyAsync(a => a.UserId == _currentUser.Id, ct);

        if (userAlreadyLinked)
            return Result.Failure(new[] { "Ya tienes un perfil de autor vinculado a tu cuenta." });

        var author = await _context.Authors.FirstOrDefaultAsync(a => a.Id == authorId, ct);
        if (author == null)
            return Result.Failure(new[] { "Autor no encontrado." });

        if (author.UserId != null)
            return Result.Failure(new[] { "Este autor ya está vinculado a otra cuenta." });

        author.UserId = _currentUser.Id;
        await _context.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<List<AuthorSearchDto>> SearchAsync(string q, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new List<AuthorSearchDto>();

        var term = q.Trim().ToLower();

        return await _context.Authors
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(term))
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new AuthorSearchDto(a.Id, a.Name))
            .ToListAsync(ct);
    }

    public async Task<List<CoauthorSearchDto>> SearchCoauthorsAsync(string q, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q) || q.Trim().Length < 2)
            return new List<CoauthorSearchDto>();

        var term = q.Trim().ToLower();

        var authors = await _context.Authors
            .AsNoTracking()
            .Where(a => a.Name.ToLower().Contains(term))
            .OrderBy(a => a.Name)
            .Take(10)
            .Select(a => new CoauthorSearchDto(a.Id, a.Name, "author"))
            .ToListAsync(ct);

        var userIdsWithAuthor = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId != null)
            .Select(a => a.UserId!)
            .ToListAsync(ct);

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
                (u.UserName + " " + u.UserLastName1 + (u.UserLastName2 != null ? " " + u.UserLastName2 : "")).Trim(),
                "user"))
            .ToListAsync(ct);

        return authors.Concat(users).ToList();
    }

    public async Task<PotentialAuthorMatchesDto> GetPotentialAuthorMatchesAsync(CancellationToken ct = default)
    {
        var alreadyLinked = await _context.Authors
            .AnyAsync(a => a.UserId == _currentUser.Id, ct);

        if (alreadyLinked)
            return new PotentialAuthorMatchesDto(new List<PotentialAuthorMatchDto>(), new List<PotentialAuthorMatchDto>());

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == _currentUser.Id, ct);

        if (user == null)
            return new PotentialAuthorMatchesDto(new List<PotentialAuthorMatchDto>(), new List<PotentialAuthorMatchDto>());

        var canonicalName = string.IsNullOrEmpty(user.UserLastName2)
            ? $"{user.UserName} {user.UserLastName1}"
            : $"{user.UserName} {user.UserLastName1} {user.UserLastName2}";

        var firstNameLower = user.UserName.ToLower();
        var lastName1Lower = user.UserLastName1.ToLower();

        var candidates = await _context.Authors
            .AsNoTracking()
            .Where(a => a.UserId == null
                && a.Name.ToLower().Contains(lastName1Lower)
                && a.Name.ToLower().Contains(firstNameLower))
            .Select(a => new { a.Id, a.Name })
            .ToListAsync(ct);

        var exact = candidates
            .Where(a => string.Equals(a.Name.Trim(), canonicalName.Trim(), System.StringComparison.OrdinalIgnoreCase))
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
