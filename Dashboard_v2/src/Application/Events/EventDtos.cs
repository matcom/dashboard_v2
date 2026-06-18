namespace Dashboard_v2.Application.Events;

public record CountryDto(int Id, string Name);

public record EventTypeDto(int Id, string Name);

public record EventDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public int CountryId { get; init; }
    public string CountryName { get; init; } = default!;
    public int EventTypeId { get; init; }
    public string EventTypeName { get; init; } = default!;
    public List<string> Institutions { get; init; } = [];
    public int PresentationCount { get; init; }
}

public record PresentationDto
{
    public int Id { get; init; }
    public string Name { get; init; } = default!;
    public int EventId { get; init; }
    public string EventName { get; init; } = default!;
    public List<string> Authors { get; init; } = [];
}
