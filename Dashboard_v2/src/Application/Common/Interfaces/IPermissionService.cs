namespace Dashboard_v2.Application.Common.Interfaces;

/// <summary>
/// Servicio para gestión de permisos granulares sobre recursos
/// </summary>
public interface IPermissionService
{
    // ============ Verificación de Permisos ============
    
    /// <summary>
    /// Verifica si un usuario tiene un permiso específico sobre un recurso
    /// </summary>
    Task<bool> HasPermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si un usuario puede acceder a un campo específico de un recurso
    /// </summary>
    Task<bool> CanAccessFieldAsync(string userId, int resourceId, string fieldName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene los campos permitidos para un usuario sobre un recurso
    /// </summary>
    /// <returns>Array de nombres de campos, o null si tiene acceso a todos</returns>
    Task<string[]?> GetAllowedFieldsAsync(string userId, int resourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Verifica si el usuario es dueño del recurso
    /// </summary>
    Task<bool> IsOwnerAsync(string userId, int resourceId, CancellationToken cancellationToken = default);
    
    // ============ Gestión de Grants ============
    
    /// <summary>
    /// Otorga un permiso a un usuario sobre un recurso
    /// </summary>
    Task<int> GrantPermissionAsync(
        string userId, 
        int resourceId, 
        string permissionName,
        string grantedBy,
        DateTimeOffset? expiresAt = null,
        string[]? fieldsAllowed = null,
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoca un permiso específico de un usuario sobre un recurso
    /// </summary>
    Task<bool> RevokePermissionAsync(string userId, int resourceId, string permissionName, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Revoca todos los permisos de un usuario sobre un recurso
    /// </summary>
    Task<int> RevokeAllPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Desactiva un grant específico por ID
    /// </summary>
    Task<bool> DeactivateGrantAsync(int grantId, CancellationToken cancellationToken = default);
    
    // ============ Consultas ============
    
    /// <summary>
    /// Obtiene todos los permisos activos de un usuario sobre un recurso
    /// </summary>
    Task<List<string>> GetUserPermissionsAsync(string userId, int resourceId, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los recursos sobre los que un usuario tiene algún permiso
    /// </summary>
    Task<List<int>> GetUserResourcesAsync(string userId, string? permissionName = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Obtiene todos los usuarios que tienen permisos sobre un recurso
    /// </summary>
    Task<List<string>> GetResourceUsersAsync(int resourceId, string? permissionName = null, CancellationToken cancellationToken = default);
    
    // ============ Limpieza ============
    
    /// <summary>
    /// Elimina todos los grants expirados
    /// </summary>
    Task<int> CleanupExpiredGrantsAsync(CancellationToken cancellationToken = default);
}
