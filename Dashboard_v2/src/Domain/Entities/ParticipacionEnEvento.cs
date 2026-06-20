using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

public class ParticipacionEnEvento : BaseAuditableEntity
{
    public string UserId { get; set; } = default!;
    public User User { get; set; } = null!;

    public int EventId { get; set; }
    public Event Event { get; set; } = null!;

    public DateOnly Fecha { get; set; }

}
