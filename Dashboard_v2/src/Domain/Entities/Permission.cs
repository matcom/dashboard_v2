namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Catálogo de permisos disponibles en el sistema
/// </summary>
public class Permission
{
    public int Id { get; set; }
    
    /// <summary>
    /// Nombre único del permiso: 'read', 'write', 'delete', 'approve', 'share', etc.
    /// </summary>
    public string Name { get; set; } = default!;
    
    /// <summary>
    /// Tipo de recurso al que aplica este permiso (null = todos los tipos)
    /// </summary>
    public string? ResourceType { get; set; }
    
    /// <summary>
    /// Descripción del permiso
    /// </summary>
    public string? Description { get; set; }
    
    // Navigation properties
    public ICollection<ResourceGrant> Grants { get; set; } = new List<ResourceGrant>();
}
