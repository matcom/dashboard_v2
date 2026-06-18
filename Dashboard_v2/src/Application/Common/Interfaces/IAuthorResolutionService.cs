using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Common.Interfaces;

/// <summary>
/// Resuelve o crea el perfil de autor vinculado a un usuario registrado.<br/>
/// Centraliza la lógica de find-or-create para evitar duplicación entre handlers.
/// </summary>
public interface IAuthorResolutionService
{
    /// <summary>
    /// Retorna el <see cref="Author"/> vinculado al <paramref name="userId"/> dado.<br/>
    /// Si no existe perfil de autor aún, lo crea automáticamente usando los campos de nombre del usuario.<br/>
    /// Retorna <c>null</c> si el usuario no existe en la base de datos.
    /// </summary>
    Task<Author?> GetOrCreateForUserAsync(string userId, CancellationToken cancellationToken = default);
}
