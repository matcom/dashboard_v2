namespace Dashboard_v2.Domain.Entities;

public class Award
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public AwardType AwardType { get; set; }

    public ICollection<UserAwarded> UserAwardeds { get; set; } = new List<UserAwarded>();
}