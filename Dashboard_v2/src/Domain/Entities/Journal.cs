namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Especialización de Publicación: cuando la publicación es una Revista (MERX: Revista).
/// 
/// pub_id aparece como atributo punteado (llave heredada) en el MERX → aquí es PublicationId (PK y FK).
/// Atributos propios: Database, GroupName, Quartile (MERX: database, group, cuartil).
/// 
/// Cardinalidad: una Publicación puede ser 0 o 1 Revista (1:1 opcional).
/// </summary>
public class Journal
{
    /// <summary>
    /// PK y FK a Publicaciones. Llave heredada del MERX (pub_id punteado).
    /// </summary>
    public int PublicationId { get; set; }

    /// <summary>
    /// Base de datos donde está indexada la revista (MERX: database).
    /// </summary>
    public string? Database { get; set; }

    /// <summary>
    /// Grupo o categoría de la revista (MERX: group).
    /// </summary>
    public string? GroupName { get; set; }

    /// <summary>
    /// Cuartil de la revista (Q1, Q2, Q3, Q4) (MERX: cuartil).
    /// </summary>
    public string? Quartile { get; set; }

    // Navigation
    public Publication Publication { get; set; } = default!;
}
