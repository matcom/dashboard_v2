namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Catálogo de tipos de publicación (MERX: Tipo de publicación).
/// Atributos llave: Id (tp_id). Atributos: Name (nombre).
/// </summary>
public class PublicationType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    // Navigation
    public ICollection<Publication> Publications { get; set; } = new List<Publication>();
}
