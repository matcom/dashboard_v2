using Dashboard_v2.Domain.Entities;

namespace Dashboard_v2.Application.Awards;

/// <summary>Premio recibido por el usuario actual (fila de UserAwarded + detalles del premio).</summary>
public record AwardDto
{
    /// <summary>Id de la fila UserAwarded (se usa para PUT/DELETE).</summary>
    public int Id { get; init; }
    public string AwardName { get; init; } = default!;
    /// <summary>Valor numérico del enum AwardType.</summary>
    public int AwardType { get; init; }
    public int Year { get; init; }
    public DateTime AwardedAt { get; init; }
}
