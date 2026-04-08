using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Representa un recurso genérico del sistema (documento, proyecto, reporte, etc.)
/// </summary>
public class Resource : BaseAuditableEntity
{
    // Id is inherited from BaseEntity as int
    
    /// <summary>
    /// Tipo de recurso: 'Document', 'Project', 'Report', 'ClientRecord', etc.
    /// </summary>
    public string Type { get; set; } = default!;
    
    /// <summary>
    /// ID del usuario propietario del recurso (referencia a AspNetUsers)
    /// </summary>
    public string OwnerId { get; set; } = default!;
    
    /// <summary>
    /// Nombre descriptivo del recurso
    /// </summary>
    public string Name { get; set; } = default!;
    
    /// <summary>
    /// Metadatos adicionales en formato JSON
    /// </summary>
    public string? Metadata { get; set; }

    // Navigation properties
    public User Owner { get; set; } = default!;
}
