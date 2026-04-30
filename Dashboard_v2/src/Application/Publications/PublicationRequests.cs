using Dashboard_v2.Domain.Enums;
using System.Collections.Generic;

namespace Dashboard_v2.Application.Publications;

public record CreatePublicationRequest
{
    public string Title { get; init; } = default!;
    public string PublicationData { get; init; } = default!;
    public PublicationType PublicationType { get; init; }
    public string? UrlDoi { get; init; }
    public List<string> AdditionalAuthorIds { get; init; } = [];
    public List<string> AdditionalAuthorNames { get; init; } = [];
    public List<string> AdditionalUserIds { get; init; } = [];
    public string? Index { get; init; }
    public string? DataBase { get; init; }
    public int? Group { get; init; }
    public string? Cuartil { get; init; }
    /// <summary>
    /// Si es verdadero, al guardar se intentará resolver la base de datos
    /// y el grupo (y cuartil para grupo 1) a partir de ISSN obtenidos
    /// desde CrossRef. Valor por defecto: false.
    /// </summary>
    public bool ResolveDatabaseFromCrossRef { get; init; } = false;
    public string? ProyectoId { get; init; }
}

public record UpdatePublicationRequest
{
    public string Id { get; init; } = default!;
    public string Title { get; init; } = default!;
    public string PublicationData { get; init; } = default!;
    public PublicationType PublicationType { get; init; }
    public string? UrlDoi { get; init; }
    public List<string> AdditionalAuthorIds { get; init; } = [];
    public List<string> AdditionalAuthorNames { get; init; } = [];
    public List<string> AdditionalUserIds { get; init; } = [];
    public string? Index { get; init; }
    public string? DataBase { get; init; }
    public int? Group { get; init; }
    public string? Cuartil { get; init; }
    /// <summary>
    /// Indica si debe intentar resolver la base de datos/grupo desde CrossRef
    /// usando ISSN al actualizar. Útil para controlar el comportamiento desde
    /// la UI (checkbox).
    /// </summary>
    public bool ResolveDatabaseFromCrossRef { get; init; } = false;
    public string? ProyectoId { get; init; }
}
