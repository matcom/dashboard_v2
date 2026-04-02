namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Especialización de Publication para publicaciones en revista (PublicationType = Diario).
/// PublicationId es a la vez PK y FK hacia Publication.
/// </summary>
public class JournalPublication
{
    public string PublicationId { get; set; } = default!;
    public int Group { get; set; }

    public Publication Publication { get; set; } = default!;

    /// <summary>Revistas donde se publica esta publicación (mínimo una).</summary>
    public ICollection<Journal> Journals { get; set; } = new List<Journal>();

    /// <summary>Bases de datos bibliográficas donde está indizada esta publicación.</summary>
    public ICollection<PublicationDatabase> Databases { get; set; } = new List<PublicationDatabase>();
}
