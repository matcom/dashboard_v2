using System.Collections.Generic;

namespace Dashboard_v2.Infrastructure.Configuration;

/// <summary>
/// Configuration options for the local-CSV provider within the publication
/// database resolution subsystem.
/// </summary>
public class PublicationDatabaseOptions
{
    /// <summary>
    /// Paths to CSV files containing ISSN → database mappings. CSV format:
    ///   issn,database[,source[,cuartil]]
    /// Example: "1234-5678,Scopus,scopus_master.csv,Q2"
    /// ISSNs are normalised (hyphens stripped) so both "1234-5678" and "12345678" work.
    /// </summary>
    public List<string> LocalMappingFiles { get; set; } = new();
}
