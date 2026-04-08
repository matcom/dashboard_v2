namespace Dashboard_v2.Application.Common.Interfaces;

/// <summary>
/// Limpieza automática de autores huérfanos.<br/>
/// Un autor es huérfano cuando no está referenciado por ninguna entidad del sistema
/// y no tiene cuenta de usuario vinculada (<c>UserId == null</c>).
/// </summary>
public interface IAuthorCleanupService
{
    /// <summary>
    /// Para cada ID de la lista verifica dinámicamente, usando el metamodelo de EF Core,
    /// si el autor sigue siendo referenciado desde alguna entidad del dominio.
    /// Si no tiene ninguna referencia y no está vinculado a un usuario registrado,
    /// lo elimina automáticamente.<br/>
    /// Esto cubre todas las entidades futuras sin necesidad de modificar este método.
    /// </summary>
    Task CleanupIfOrphanedAsync(IEnumerable<string> authorIds, CancellationToken cancellationToken = default);
}
