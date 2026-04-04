namespace Dashboard_v2.Domain.Entities;

public class EventType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<Event> Events { get; set; } = new List<Event>();
}
