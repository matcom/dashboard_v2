namespace Dashboard_v2.Domain.Entities;
public class AwardType
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;

    public ICollection<Award> Awards { get; set; } = new List<Award>();
}
