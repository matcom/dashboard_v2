using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Application.Publications;

/// <summary>
/// Datos de especialización transferidos al <see cref="Dashboard_v2.Application.Common.Interfaces.IPublicationSpecializationService"/>.<br/>
/// Actúa como objeto de parámetro (Parameter Object Pattern) para desacoplar la interfaz
/// del servicio de los tipos de comando concretos y cumplir con DIP.
/// </summary>
public record PublicationSpecializationData(
    PublicationType PublicationType,
    // ── Revista ──────────────────────────────────────────────────────────────
    string? JournalName,
    string? JournalISSN,
    string? JournalEISSN,
    // ── Base de datos ─────────────────────────────────────────────────────────
    string? DatabaseName,
    string? DatabaseUrl,
    // ── Clasificación ────────────────────────────────────────────────────────
    int? Group,
    /// <summary>Obligatorio cuando DatabaseName contiene "scopus" (insensible a mayúsculas).</summary>
    Cuartil? Cuartil,
    // ── Indexación (tipos no-Diario) ──────────────────────────────────────────
    string? Index);
