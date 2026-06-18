namespace Dashboard_v2.Domain.Entities;

public class AuthorPresentation
{
    public string AuthorId { get; set; } = default!;
    public Author Author { get; set; } = default!;

    public int PresentationId { get; set; }
    public Presentation Presentation { get; set; } = default!;
}
