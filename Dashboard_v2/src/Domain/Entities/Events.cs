namespace Dashboard_v2.Domain.Entities;

public class Event
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    /// <summary>Instituciones organizadoras.</summary>
    public List<string> Institutions { get; set; } = [];

    // País donde se desarrolló el evento (1 país por evento según el modelo)
    public int CountryId { get; set; }
    public Country Country { get; set; } = null!;

    public int EventTypeId { get; set; }
    public EventType EventType { get; set; } = default!;

    public ICollection<Presentation> Presentations { get; set; } = new List<Presentation>();
}