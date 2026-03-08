using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Publicación académica o científica (MERX: Publicación).
/// Atributos llave: Id (pub_id). Atributos: Title, AuthorRelation, PublicationDate.
/// 
/// Vincula con Resources para el sistema de permisos (Type = "Publication").
/// Puede tener especializaciones opcionales: Journal (Revista) y/o IndexedPublication.
/// </summary>
public class Publication : BaseAuditableEntity
{
    // Id (pub_id) is inherited from BaseEntity as int

    /// <summary>
    /// FK al sistema de permisos. Una Publicación ES un Resource de tipo "Publication".
    /// </summary>
    public int ResourceId { get; set; }

    /// <summary>
    /// Título de la publicación (MERX: title).
    /// </summary>
    public string Title { get; set; } = default!;

    /// <summary>
    /// Relación de autoría / afiliación del autor (MERX: author_relation).
    /// </summary>
    public string? AuthorRelation { get; set; }

    /// <summary>
    /// Fecha de publicación (MERX: pub_data).
    /// </summary>
    public DateOnly? PublicationDate { get; set; }

    /// <summary>
    /// FK al tipo de publicación. Cardinalidad: 0,* → 1,1 (cada publicación tiene un tipo).
    /// </summary>
    public int PublicationTypeId { get; set; }

    // Navigation properties
    public Resource Resource { get; set; } = default!;
    public PublicationType PublicationType { get; set; } = default!;

    /// <summary>
    /// Especialización opcional: si esta publicación es una Revista.
    /// MERX: Revista es subconjunto de Publicación (pub_id heredado punteado).
    /// </summary>
    public Journal? Journal { get; set; }

    /// <summary>
    /// Especialización opcional: si esta publicación está indexada.
    /// MERX: Publicación indexada es subconjunto de Publicación (pub_id heredado punteado).
    /// </summary>
    public IndexedPublication? IndexedPublication { get; set; }
}
