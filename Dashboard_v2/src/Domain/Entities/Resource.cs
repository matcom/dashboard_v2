using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Base de indexación para el sistema de permisos.
/// Cada entidad de negocio (Publicación, Proyecto, etc.) referencia una fila de esta tabla.
/// No almacena datos de negocio — solo la etiqueta de tipo y el propietario.
/// </summary>
public class Resource : BaseAuditableEntity
{
    // Id is inherited from BaseEntity as int

    /// <summary>
    /// Tipo de recurso: 'Publication', 'Project', 'Patent', etc.
    /// Etiqueta técnica inmutable — se asigna al crear el recurso y nunca cambia.
    /// </summary>
    public string Type { get; set; } = default!;

    /// <summary>
    /// ID del usuario propietario del recurso.
    /// </summary>
    public string OwnerId { get; set; } = default!;

    // Navigation properties
    public ICollection<ResourceGrant> Grants { get; set; } = new List<ResourceGrant>();
}
