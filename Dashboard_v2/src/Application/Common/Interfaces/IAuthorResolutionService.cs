using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Common.Interfaces;

/// <summary>
/// Resuelve autores a partir de distintas fuentes de referencia:
/// IDs de autores existentes, nombres libres o IDs de usuarios registrados.<br/>
/// Centraliza la lógica "find-or-create" de autores para cumplir SRP (Single Responsibility Principle)
/// y evitar duplicación entre los handlers de publicaciones, presentaciones y similares.
/// </summary>
public interface IAuthorResolutionService
{
    /// <summary>
    /// Dado el ID de un usuario del sistema, retorna su perfil de autor vinculado.
    /// Si no existe, lo crea automáticamente con el nombre completo del usuario.<br/>
    /// Retorna <c>null</c> si el usuario no existe en la BD.
    /// </summary>
    Task<Author?> ResolveOrCreateByUserIdAsync(string userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Construye la lista de <see cref="AuthorPublication"/> de coautores adicionales a partir
    /// de tres fuentes complementarias:<br/>
    /// - <paramref name="existingAuthorIds"/>: autores ya existentes en BD.<br/>
    /// - <paramref name="newAuthorNames"/>: nombres libres (se crean como autores sin cuenta).<br/>
    /// - <paramref name="userIds"/>: IDs de usuarios registrados (find-or-create).<br/>
    /// El <paramref name="currentAuthorId"/> se excluye automáticamente para no duplicarlo.
    /// </summary>
    Task<List<AuthorPublication>> ResolveCoauthorsAsync(
        string currentAuthorId,
        IEnumerable<string> existingAuthorIds,
        IEnumerable<string> newAuthorNames,
        IEnumerable<string> userIds,
        CancellationToken cancellationToken = default);
}
