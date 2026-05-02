using System.Threading;
using System.Threading.Tasks;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Common;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Application.Common;

public sealed class AuthorResolutionService : IAuthorResolutionService
{
    private readonly IApplicationDbContext _context;

    public AuthorResolutionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Author?> GetOrCreateForUserAsync(string userId, CancellationToken cancellationToken = default)
    {
        var author = await _context.Authors
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (author != null)
            return author;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        author = Author.Create(
            lastName : (user.UserLastName1 + (string.IsNullOrEmpty(user.UserLastName2) ? string.Empty : " " + user.UserLastName2)).Trim(),
            firstName: user.UserName.Trim());
        author.UserId = user.Id;

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        return author;
    }

    public async Task<Author> ResolveByNameAsync(string nameString, CancellationToken cancellationToken = default)
    {
        var (lastName, firstName) = AuthorNameParser.Parse(nameString);
        var searchKey = TextNormalizer.Normalize(nameString.Trim());
        var lastKey   = TextNormalizer.Normalize(lastName);

        // 1. Match on the normalized search key (tolerates diacritics & case).
        var existing = await _context.Authors
            .FirstOrDefaultAsync(a => a.SearchKey == searchKey, cancellationToken);

        if (existing != null)
            return existing;

        // 2. Structured match: normalized LastName + normalized FirstName (both non-empty).
        if (!string.IsNullOrWhiteSpace(firstName))
        {
            var firstKey = TextNormalizer.Normalize(firstName);
            existing = await _context.Authors
                .FirstOrDefaultAsync(
                    a => a.LastNameKey == lastKey && a.FirstNameKey == firstKey,
                    cancellationToken);

            if (existing != null)
                return existing;
        }

        // 3. No match — create and persist a new author.
        var author = Author.Create(lastName, firstName);
        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);
        return author;
    }
}
