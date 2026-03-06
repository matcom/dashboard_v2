using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Define qué permisos tiene un rol sobre un tipo de recurso específico
/// Esto permite configurar políticas base por rol (ej: "Manager" tiene permisos read/write/approve en "Document")
/// </summary>
public class RolePermission : BaseAuditableEntity
{
    // Id is inherited from BaseEntity as int
    
    /// <summary>
    /// ID del rol (referencia a AspNetRoles)
    /// </summary>
    public string RoleId { get; set; } = default!;
    
    /// <summary>
    /// ID del permiso
    /// </summary>
    public int PermissionId { get; set; }
    
    /// <summary>
    /// Tipo de recurso al que aplica (null = aplica a todos los tipos)
    /// Ejemplo: "Document", "Project", "Report"
    /// </summary>
    public string? ResourceType { get; set; }
    
    /// <summary>
    /// Indica si este permiso de rol está activo
    /// </summary>
    public bool IsActive { get; set; } = true;
    
    // Navigation properties
    public Permission Permission { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
