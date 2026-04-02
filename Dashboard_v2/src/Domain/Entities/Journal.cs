namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Revista académica donde se publica una <see cref="JournalPublication"/>.
/// </summary>
public class Journal
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = default!;
    public string? ISSN { get; set; }
    public string? EISSN { get; set; }

    /// <summary>FK hacia la publicación en revista a la que pertenece esta revista.</summary>
    public string JournalPublicationId { get; set; } = default!;
    public JournalPublication JournalPublication { get; set; } = default!;

    /// <summary>Solo presente si la revista está indizada en Scopus.</summary>
    public ScopusJournal? ScopusJournal { get; set; }
}
