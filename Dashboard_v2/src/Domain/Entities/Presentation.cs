namespace Dashboard_v2.Domain.Entities;

public class Presentation
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public ICollection<AuthorPresentation> AuthorPresentations { get; set; } = new List<AuthorPresentation>();
}