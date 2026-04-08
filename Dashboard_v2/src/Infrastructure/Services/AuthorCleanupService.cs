using System.Linq.Expressions;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Dashboard_v2.Infrastructure.Services;

/// <summary>
/// Usa el metamodelo de EF Core para descubrir en tiempo de ejecución todas las entidades
/// que referencian a <see cref="Author"/> mediante FK.<br/>
/// Si un autor no tiene ninguna referencia y no está vinculado a un usuario registrado,
/// se elimina automáticamente. Cuando se añadan nuevas entidades que referencien a Author
/// (p.ej. EventoParticipante), este servicio las detectará sin necesidad de modificación.
/// </summary>
public class AuthorCleanupService : IAuthorCleanupService
{
    private readonly ApplicationDbContext _context;

    public AuthorCleanupService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task CleanupIfOrphanedAsync(
        IEnumerable<string> authorIds,
        CancellationToken cancellationToken = default)
    {
        // Descubrir todas las FKs que apuntan a Author una sola vez
        var referencingFKs = _context.Model
            .FindEntityType(typeof(Author))!
            .GetReferencingForeignKeys()
            .ToList();

        // Cachear los MethodInfo genéricos para no repetir reflexión por cada autor
        var setMethodBase = typeof(DbContext)
            .GetMethods()
            .First(m => m.Name == "Set" && m.IsGenericMethod && m.GetParameters().Length == 0);

        var anyAsyncMethodBase = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m =>
                m.Name == nameof(EntityFrameworkQueryableExtensions.AnyAsync) &&
                m.GetParameters().Length == 3);

        foreach (var authorId in authorIds.Distinct())
        {
            var author = await _context.Authors
                .FirstOrDefaultAsync(a => a.Id == authorId, cancellationToken);

            // No existe o está vinculado a un usuario registrado → nunca autoeliminar
            if (author is null || author.UserId is not null)
                continue;

            var hasReferences = false;

            foreach (var fk in referencingFKs)
            {
                var clrType = fk.DeclaringEntityType.ClrType;
                var fkPropName = fk.Properties.First().Name;

                // dbContext.Set<clrType>()
                var set = setMethodBase.MakeGenericMethod(clrType).Invoke(_context, null)!;

                // Construir: e => EF.Property<string>(e, fkPropName) == authorId
                var param = Expression.Parameter(clrType, "e");
                var efProp = Expression.Call(
                    typeof(EF),
                    nameof(EF.Property),
                    [typeof(string)],
                    param,
                    Expression.Constant(fkPropName));

                var predicate = Expression.Lambda(
                    Expression.Equal(efProp, Expression.Constant(authorId)),
                    param);

                // EntityFrameworkQueryableExtensions.AnyAsync<clrType>(set, predicate, ct)
                var task = (Task<bool>)anyAsyncMethodBase
                    .MakeGenericMethod(clrType)
                    .Invoke(null, [set, predicate, cancellationToken])!;

                hasReferences = await task;
                if (hasReferences) break;
            }

            if (!hasReferences)
                _context.Authors.Remove(author);
        }

        if (_context.ChangeTracker.HasChanges())
            await _context.SaveChangesAsync(cancellationToken);
    }
}
