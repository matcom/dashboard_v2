using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dashboard_v2.Application.Publications;
using Microsoft.Extensions.Logging;

namespace Dashboard_v2.Infrastructure.Services.Providers;

/// <summary>
/// Lightweight provider that reads one or more CSV files mapping ISSN -> database.
/// This provider is intended for offline, authoritative lists (e.g. cached SJR, Scielo
/// lists, DOAJ dumps). CSV format expected: issn,database,source,cuartil
/// </summary>
internal class LocalCsvPublicationDatabaseProvider
{
    private readonly Microsoft.Extensions.Logging.ILogger _logger;
    private readonly Dictionary<string, (string database, string? cuartil, string? source)> _map = new(StringComparer.OrdinalIgnoreCase);

    public LocalCsvPublicationDatabaseProvider(IEnumerable<string> csvPaths, Microsoft.Extensions.Logging.ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        foreach (var p in csvPaths ?? Array.Empty<string>())
        {
            try
            {
                if (!File.Exists(p))
                {
                    _logger.LogDebug("Local mapping CSV not found: {Path}", p);
                    continue;
                }

                using var sr = new StreamReader(p);
                string? line;
                while ((line = sr.ReadLine()) != null)
                {
                    if (string.IsNullOrWhiteSpace(line)) continue;
                    var parts = line.Split(',');
                    if (parts.Length < 2) continue;
                    var issn = NormalizeIssn(parts[0]);
                    if (string.IsNullOrWhiteSpace(issn)) continue;
                    var database = parts[1].Trim();
                    var source = parts.Length > 2 ? parts[2].Trim() : Path.GetFileName(p);
                    var cuartil = parts.Length > 3 ? parts[3].Trim() : null;
                    _map[issn] = (database, string.IsNullOrWhiteSpace(cuartil) ? null : cuartil, source);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error loading local CSV mapping {Path}", p);
            }
        }
    }

    public ValueTask<PublicationDatabaseMatchDto?> TryLookupByIssnsAsync(IEnumerable<string> issns, CancellationToken ct)
    {
        foreach (var issn in issns)
        {
            var key = NormalizeIssn(issn);
            if (string.IsNullOrWhiteSpace(key)) continue;
            if (_map.TryGetValue(key, out var v))
            {
                return ValueTask.FromResult<PublicationDatabaseMatchDto?>(new PublicationDatabaseMatchDto
                {
                    DatabaseName = v.database,
                    Cuartil = v.cuartil,
                    Source = v.source,
                    Confidence = 0.95,
                    Group = MapDatabaseToGroup(v.database)
                });
            }
        }

        return ValueTask.FromResult<PublicationDatabaseMatchDto?>(null);
    }

    private static string NormalizeIssn(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        s = s.Trim().ToUpperInvariant();
        return s;
    }

    private static int? MapDatabaseToGroup(string db)
    {
        if (string.IsNullOrWhiteSpace(db)) return null;
        var d = db.ToLowerInvariant();
        // Group 1
        if (d.Contains("scopus") || d.Contains("scimago") || d.Contains("web of science") || d.Contains("scie") || d.Contains("ssci") || d.Contains("ahci"))
            return 1;
        // Group 2
        if (d.Contains("scielo") || d.Contains("medline") || d.Contains("emerging") || d.Contains("chemical") || d.Contains("biosis") || d.Contains("compendex") || d.Contains("cab") || d.Contains("inspec"))
            return 2;
        // Group 3
        if (d.Contains("doaj") || d.Contains("lilacs") || d.Contains("redalyc") || d.Contains("latindex") || d.Contains("ime") || d.Contains("icyt") || d.Contains("periodica") || d.Contains("clase"))
            return 3;
        // else group 4
        return 4;
    }
}
