using System.Collections.Generic;

namespace Dashboard_v2.Application.Publications;

/// <summary>
/// Result of attempting to resolve which bibliographic database(s) a journal
/// (identified by ISSN) is covered by, together with the derived Group and
/// optional cuartil information.
/// </summary>
public class PublicationDatabaseMatchDto
{
    /// <summary>
    /// ISSNs returned by CrossRef for this journal. Populated even when no
    /// database provider found a match, so the client can display them.
    /// </summary>
    public List<string> Issns { get; set; } = [];

    /// <summary>
    /// Primary database name determined for the journal, e.g. "Scopus", "DOAJ",
    /// "SciELO", "MEDLINE". Null when no match found.
    /// </summary>
    public string? DatabaseName { get; set; }

    /// <summary>
    /// Determined group according to local policy (1..4). Null when unknown.
    /// </summary>
    public int? Group { get; set; }

    /// <summary>
    /// When available, cuartil value such as "Q1".."Q4" (from SJR or similar).
    /// </summary>
    public string? Cuartil { get; set; }

    /// <summary>
    /// Source that provided the match (e.g. "LocalCsv:scopus.csv" or "DOAJ API").
    /// </summary>
    public string? Source { get; set; }

    /// <summary>
    /// Confidence score (0..1) for heuristic ranking of multiple matches.
    /// </summary>
    public double Confidence { get; set; } = 0.0;
}
