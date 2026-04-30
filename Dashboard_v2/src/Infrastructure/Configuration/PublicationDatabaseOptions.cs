using System.Collections.Generic;

namespace Dashboard_v2.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the publication database resolution subsystem.
/// Allows pointing to local CSV mapping files and tuning provider order.
/// </summary>
public class PublicationDatabaseOptions
{
    /// <summary>
    /// Paths to CSV files containing ISSN -> database mappings. CSV format:
    /// issn,database,source,cuartil
    /// Example line: "1234-5678,Scopus,scopus_master.csv,Q2"
    /// The resolver will load these files if present and use them with priority.
    /// </summary>
    public List<string> LocalMappingFiles { get; set; } = new();

    /// <summary>
    /// Provider discovery order. Supported values: "LocalCsv", "DOAJ", "NLM".
    /// </summary>
    public List<string> ProviderOrder { get; set; } = new() { "LocalCsv", "DOAJ", "NLM" };
}
