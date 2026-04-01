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
        => ValidateFields(data.PublicationType, data.JournalName, data.DataBase,
                          data.Group, data.Cuartil, data.Index);

    /// <inheritdoc/>
    public void AttachSpecialization(Publication publication, PublicationSpecializationData data)
    {
        if (data.PublicationType == PublicationType.Diario)
        {
            publication.JournalPublication = new JournalPublication
            {
                PublicationId = publication.Id,
                Name = data.JournalName!.Trim(),
                DataBase = data.DataBase!.Trim(),
                Group = data.Group!.Value,
                JournalGroup1Publication = data.Group == 1
                    ? new JournalGroup1Publication
                    {
                        PublicationId = publication.Id,
                        Cuartil = data.Cuartil!.Value
                    }
                    : null
            };
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
        var isNowJournal = data.PublicationType == PublicationType.Diario;
        var wasJournal = publication.JournalPublication != null;

        // Limpiar especialización anterior si el tipo cambió
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

    private static string? ValidateFields(
        PublicationType type,
        string? journalName,
        string? dataBase,
        int? group,
        Cuartil? cuartil,
        string? index)
    {
        if (type == PublicationType.Diario)
        {
            if (string.IsNullOrWhiteSpace(journalName) ||
                string.IsNullOrWhiteSpace(dataBase) ||
                group is null or < 1 or > 4)
                return "Datos de la revista son obligatorios: nombre, base de datos y grupo (1–4).";

            if (group == 1 && (cuartil is null || !Enum.IsDefined(typeof(Cuartil), cuartil.Value)))
                return "Cuartil es obligatorio para revistas de grupo 1.";
        }
        else if (string.IsNullOrWhiteSpace(index))
        {
            return "La indexación es obligatoria para este tipo de publicación.";
        }

        return null;
    }

    private void RemoveJournalSpecialization(Publication publication)
    {
        if (publication.JournalPublication!.JournalGroup1Publication != null)
            _context.JournalGroup1Publications.Remove(publication.JournalPublication.JournalGroup1Publication);

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
                Name = data.JournalName!.Trim(),
                DataBase = data.DataBase!.Trim(),
                Group = data.Group!.Value
            };
        }
        else
        {
            publication.JournalPublication.Name = data.JournalName!.Trim();
            publication.JournalPublication.DataBase = data.DataBase!.Trim();
            publication.JournalPublication.Group = data.Group!.Value;
        }

        if (data.Group == 1)
        {
            if (publication.JournalPublication.JournalGroup1Publication == null)
            {
                var g1 = new JournalGroup1Publication
                {
                    PublicationId = publication.Id,
                    Cuartil = data.Cuartil!.Value
                };
                publication.JournalPublication.JournalGroup1Publication = g1;
                _context.JournalGroup1Publications.Add(g1);
            }
            else
            {
                publication.JournalPublication.JournalGroup1Publication.Cuartil = data.Cuartil!.Value;
            }
        }
        else if (publication.JournalPublication.JournalGroup1Publication != null)
        {
            _context.JournalGroup1Publications.Remove(publication.JournalPublication.JournalGroup1Publication);
            publication.JournalPublication.JournalGroup1Publication = null;
        }
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
