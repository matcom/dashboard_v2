namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Base de datos bibliográfica donde se encuentra indizada una <see cref="JournalPublication"/>.
/// </summary>
public class PublicationDatabase
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = default!;
    public string? Url { get; set; }

    /// <summary>FK hacia la publicación en revista a la que pertenece esta base de datos.</summary>
    public string JournalPublicationId { get; set; } = default!;
    public JournalPublication JournalPublication { get; set; } = default!;
}
