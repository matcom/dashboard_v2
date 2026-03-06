namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Especialización de Publicación: cuando la publicación está indexada (MERX: Publicación indexada).
///
/// pub_id aparece como atributo punteado (llave heredada) en el MERX → aquí es PublicationId (PK y FK).
/// Atributos propios: IndexName (MERX: Index).
///
/// Cardinalidad: una Publicación puede ser 0 o 1 Publicación indexada (1:1 opcional).
/// Una publicación puede ser simultáneamente Revista Y Publicación indexada (especialización no disjunta).
/// </summary>
public class IndexedPublication
{
    /// <summary>
    /// PK y FK a Publicaciones. Llave heredada del MERX (pub_id punteado).
    /// </summary>
    public int PublicationId { get; set; }

    /// <summary>
    /// Nombre del índice o base de datos donde está indexada (MERX: Index).
    /// Ejemplo: Scopus, Web of Science, PubMed, etc.
    /// </summary>
    public string? IndexName { get; set; }

    // Navigation
    public Publication Publication { get; set; } = default!;
}
