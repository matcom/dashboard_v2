using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Representa la asignación de un permiso específico a un usuario sobre un recurso
/// </summary>
public class ResourceGrant : BaseAuditableEntity
{
    // Id is inherited from BaseEntity as int
    
    /// <summary>
    /// ID del usuario que recibe el permiso (referencia a AspNetUsers)
    /// </summary>
    public string UserId { get; set; } = default!;
    
    /// <summary>
    /// ID del recurso sobre el que se otorga el permiso
    /// </summary>
    public int ResourceId { get; set; }
    
    /// <summary>
    /// ID del permiso otorgado
    /// </summary>
    public int PermissionId { get; set; }
    
    /// <summary>
    /// ID del usuario que otorgó el permiso
    /// </summary>
    public string? GrantedBy { get; set; }
    
    /// <summary>
    /// Fecha en que se otorgó el permiso
    /// </summary>
    public DateTimeOffset GrantedAt { get; set; }
    
    /// <summary>
    /// Fecha de expiración del permiso (null = sin expiración)
    /// </summary>
    public DateTimeOffset? ExpiresAt { get; set; }
    
    /// <summary>
    /// Campos permitidos en formato JSON array (null = todos los campos)
    /// Ejemplo: ["name", "date", "amount"]
    /// </summary>
    public string? FieldsAllowed { get; set; }
    
    /// <summary>
    /// Condiciones adicionales en formato JSON (para reglas avanzadas)
    /// </summary>
    public string? Conditions { get; set; }
    
    /// <summary>
    /// Indica si el grant está activo
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Resource Resource { get; set; } = default!;
    public Permission Permission { get; set; } = default!;
    
    // Helper methods
    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    public bool IsValid() => IsActive && !IsExpired();
}
