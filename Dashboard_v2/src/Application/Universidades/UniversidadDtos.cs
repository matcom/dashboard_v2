namespace Dashboard_v2.Application.Universidades;

public record UniversidadDto
{
    public string Id { get; init; } = default!;
    public string Nombre { get; init; } = default!;
}
