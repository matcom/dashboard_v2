using Dashboard_v2.Application.Common.Interfaces;
using Dashboard_v2.Application.Publications;
using Dashboard_v2.Domain.Entities;
using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Infrastructure.Services;

/// <inheritdoc cref="IPublicationSpecializationService"/>
public class PublicationSpecializationService : IPublicationSpecializationService
{
    private readonly IApplicationDbContext _context;

    public PublicationSpecializationService(IApplicationDbContext context)
    {
        _context = context;
    }

    /// <inheritdoc/>
    public string? Validate(PublicationSpecializationData data)
    {
        if (data.PublicationType == PublicationType.Diario)
        {
            if (string.IsNullOrWhiteSpace(data.JournalName) || data.Group is null or < 1 or > 4)
                return "Datos de la revista son obligatorios: nombre y grupo (1–4).";

            if (IsScopusDatabase(data.DatabaseName) &&
                (data.Cuartil is null || !Enum.IsDefined(typeof(Cuartil), data.Cuartil.Value)))
                return "Cuartil es obligatorio cuando la base de datos es Scopus.";
        }
        else if (string.IsNullOrWhiteSpace(data.Index))
        {
            return "La indexación es obligatoria para este tipo de publicación.";
        }

        return null;
    }

    /// <inheritdoc/>
    public void AttachSpecialization(Publication publication, PublicationSpecializationData data)
    {
        if (data.PublicationType == PublicationType.Diario)
        {
            bool isScopus = IsScopusDatabase(data.DatabaseName);
            var jp = new JournalPublication
            {
                PublicationId = publication.Id,
                Group = data.Group!.Value
            };
            jp.Journals.Add(BuildJournal(publication.Id, data, isScopus));
            if (!string.IsNullOrWhiteSpace(data.DatabaseName))
                jp.Databases.Add(BuildDatabase(publication.Id, data));
            publication.JournalPublication = jp;
        }
        else
        {
            publication.IndexedPublication = new IndexedPublication
            {
                PublicationId = publication.Id,
                Index = data.Index!.Trim()
            };
        }
    }

    /// <inheritdoc/>
    public Task ApplySpecializationUpdateAsync(
        Publication publication,
        PublicationSpecializationData data,
        CancellationToken cancellationToken = default)
    {
        bool isNowJournal = data.PublicationType == PublicationType.Diario;
        bool wasJournal = publication.JournalPublication != null;

        if (wasJournal && !isNowJournal)
        {
            RemoveJournalSpecialization(publication);
        }
        else if (!wasJournal && isNowJournal && publication.IndexedPublication != null)
        {
            _context.IndexedPublications.Remove(publication.IndexedPublication);
            publication.IndexedPublication = null;
        }

        if (isNowJournal)
            ApplyJournalUpdate(publication, data);
        else
            ApplyIndexedUpdate(publication, data);

        return Task.CompletedTask;
    }

    // ── helpers privados ──────────────────────────────────────────────────────

    private static bool IsScopusDatabase(string? name)
        => !string.IsNullOrWhiteSpace(name) &&
           name.Contains("scopus", StringComparison.OrdinalIgnoreCase);

    private static Journal BuildJournal(string jpId, PublicationSpecializationData data, bool isScopus)
    {
        var journalId = Guid.NewGuid().ToString();
        return new Journal
        {
            Id = journalId,
            JournalPublicationId = jpId,
            Name = data.JournalName!.Trim(),
            ISSN = string.IsNullOrWhiteSpace(data.JournalISSN) ? null : data.JournalISSN.Trim(),
            EISSN = string.IsNullOrWhiteSpace(data.JournalEISSN) ? null : data.JournalEISSN.Trim(),
            ScopusJournal = isScopus
                ? new ScopusJournal { JournalId = journalId, Cuartil = data.Cuartil!.Value }
                : null
        };
    }

    private static PublicationDatabase BuildDatabase(string jpId, PublicationSpecializationData data)
        => new()
        {
            JournalPublicationId = jpId,
            Name = data.DatabaseName!.Trim(),
            Url = string.IsNullOrWhiteSpace(data.DatabaseUrl) ? null : data.DatabaseUrl.Trim()
        };

    private void RemoveJournalSpecialization(Publication publication)
    {
        foreach (var j in publication.JournalPublication!.Journals.ToList())
            _context.Journals.Remove(j); // ScopusJournal cascades

        foreach (var db in publication.JournalPublication!.Databases.ToList())
            _context.PublicationDatabases.Remove(db);

        _context.JournalPublications.Remove(publication.JournalPublication);
        publication.JournalPublication = null;
    }

    private void ApplyJournalUpdate(Publication publication, PublicationSpecializationData data)
    {
        if (publication.JournalPublication == null)
        {
            publication.JournalPublication = new JournalPublication
            {
                PublicationId = publication.Id,
                Group = data.Group!.Value
            };
        }
        else
        {
            publication.JournalPublication.Group = data.Group!.Value;
            // Replace old journals and databases with new ones
            foreach (var j in publication.JournalPublication.Journals.ToList())
                _context.Journals.Remove(j);
            publication.JournalPublication.Journals.Clear();

            foreach (var db in publication.JournalPublication.Databases.ToList())
                _context.PublicationDatabases.Remove(db);
            publication.JournalPublication.Databases.Clear();
        }

        bool isScopus = IsScopusDatabase(data.DatabaseName);
        publication.JournalPublication.Journals.Add(
            BuildJournal(publication.JournalPublication.PublicationId, data, isScopus));

        if (!string.IsNullOrWhiteSpace(data.DatabaseName))
            publication.JournalPublication.Databases.Add(
                BuildDatabase(publication.JournalPublication.PublicationId, data));
    }

    private void ApplyIndexedUpdate(Publication publication, PublicationSpecializationData data)
    {
        if (publication.IndexedPublication == null)
        {
            var indexed = new IndexedPublication
            {
                PublicationId = publication.Id,
                Index = data.Index!.Trim()
            };
            publication.IndexedPublication = indexed;
            _context.IndexedPublications.Add(indexed);
        }
        else
        {
            publication.IndexedPublication.Index = data.Index!.Trim();
        }
    }
}
