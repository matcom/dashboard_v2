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
    public string? ProyectoId { get; init; }
}
