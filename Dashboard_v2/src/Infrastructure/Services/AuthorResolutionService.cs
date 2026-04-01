using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dashboard_v2.Infrastructure.Services;

/// <inheritdoc cref="IAuthorResolutionService"/>
public class AuthorResolutionService : IAuthorResolutionService
{
    private readonly IApplicationDbContext _context;

    public AuthorResolutionService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public async Task<Author?> ResolveOrCreateByUserIdAsync(
        string userId,
        CancellationToken cancellationToken = default)
    {
        var existing = await _context.Authors
            .FirstOrDefaultAsync(a => a.UserId == userId, cancellationToken);

        if (existing != null)
            return existing;

        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, cancellationToken);

        if (user == null)
            return null;

        var author = new Author
        {
            Name = BuildFullName(user.UserName, user.UserLastName1, user.UserLastName2),
            UserId = user.Id
        };

        _context.Authors.Add(author);
        await _context.SaveChangesAsync(cancellationToken);

        return author;
    }

    /// <inheritdoc/>
    public async Task<List<AuthorPublication>> ResolveCoauthorsAsync(
        string currentAuthorId,
        IEnumerable<string> existingAuthorIds,
        IEnumerable<string> newAuthorNames,
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default)
    {
        var result = new List<AuthorPublication>();

        // Autores existentes por ID
        foreach (var authorId in existingAuthorIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (authorId == currentAuthorId) continue;
            if (await _context.Authors.AnyAsync(a => a.Id == authorId, cancellationToken))
                result.Add(new AuthorPublication { AuthorId = authorId });
        }

        // Autores nuevos por nombre libre
        foreach (var name in newAuthorNames.Where(n => !string.IsNullOrWhiteSpace(n)))
        {
            result.Add(new AuthorPublication
            {
                Author = new Author { Name = name.Trim() }
            });
        }

        // Usuarios registrados (find-or-create)
        foreach (var userId in userIds.Where(id => !string.IsNullOrWhiteSpace(id)))
        {
            if (userId == currentAuthorId) continue;

            var coAuthor = await ResolveOrCreateByUserIdAsync(userId, cancellationToken);
            if (coAuthor == null) continue;

            if (result.All(ap => ap.AuthorId != coAuthor.Id))
                result.Add(new AuthorPublication { AuthorId = coAuthor.Id });
        }

        return result;
    }

    private static string BuildFullName(string firstName, string lastName1, string? lastName2)
        => $"{firstName} {lastName1}{(string.IsNullOrEmpty(lastName2) ? string.Empty : " " + lastName2)}".Trim();
}
