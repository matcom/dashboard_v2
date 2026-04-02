namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Especialización de <see cref="Journal"/> para revistas indizadas en Scopus.
/// JournalId es a la vez PK y FK hacia Journal.
/// </summary>
public class ScopusJournal
{
    public string JournalId { get; set; } = default!;
    public Cuartil Cuartil { get; set; }

    public Journal Journal { get; set; } = default!;
}
