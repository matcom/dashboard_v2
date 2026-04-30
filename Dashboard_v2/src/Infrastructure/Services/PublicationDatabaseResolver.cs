using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Publications;
using Dashboard_v2.Infrastructure.Configuration;
using Dashboard_v2.Infrastructure.Services.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dashboard_v2.Infrastructure.Services;

/// <summary>
/// Composite resolver that queries a set of providers (local CSV, DOAJ, NLM, ...)
/// in configured order and returns the best matching database/group for a journal
/// identified by one or more ISSNs.
/// </summary>
public class PublicationDatabaseResolver : IPublicationDatabaseResolver
{
    private readonly PublicationDatabaseOptions _opts;
    private readonly ILogger<PublicationDatabaseResolver> _logger;
    private readonly LocalCsvPublicationDatabaseProvider? _localProvider;

    public PublicationDatabaseResolver(IOptions<PublicationDatabaseOptions> options, ILogger<PublicationDatabaseResolver> logger)
    {
        _opts = options?.Value ?? new PublicationDatabaseOptions();
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        if (_opts.LocalMappingFiles?.Count > 0)
            _localProvider = new LocalCsvPublicationDatabaseProvider(_opts.LocalMappingFiles, logger: logger);
    }

    /// <inheritdoc/>
    public async Task<PublicationDatabaseMatchDto?> ResolveByIssnsAsync(IEnumerable<string> issns, CancellationToken ct = default)
    {
        var issnList = issns?.Where(i => !string.IsNullOrWhiteSpace(i)).Select(NormalizeIssn).Distinct().ToList() ?? new List<string>();
        if (!issnList.Any()) return null;

        _logger.LogDebug("Resolving databases for ISSNs: {Issns}", string.Join(",", issnList));

        // Provider order: LocalCsv -> DOAJ -> NLM (for now only LocalCsv implemented)
        if (_opts.ProviderOrder != null)
        {
            foreach (var providerName in _opts.ProviderOrder)
            {
                if (providerName.Equals("LocalCsv", StringComparison.OrdinalIgnoreCase) && _localProvider != null)
                {
                    var result = await _localProvider.TryLookupByIssnsAsync(issnList, ct);
                    if (result != null) return result;
                }

                // Future providers (DOAJ, NLM) will be added here.
            }
        }

        // No match found
        _logger.LogDebug("No local database mapping found for ISSNs: {Issns}", string.Join(",", issnList));
        return null;
    }

    private static string NormalizeIssn(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) return string.Empty;
        return s.Trim().ToUpperInvariant();
    }
}
