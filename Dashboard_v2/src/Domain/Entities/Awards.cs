namespace Dashboard_v2.Domain.Entities;

public class Award
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public int AwardTypeId { get; set; }
    public AwardType AwardType { get; set; } = default!;

    public ICollection<UserAwarded> UserAwardeds { get; set; } = new List<UserAwarded>();
}