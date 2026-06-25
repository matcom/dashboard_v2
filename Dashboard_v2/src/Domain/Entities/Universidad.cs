using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

public class Universidad : StringAuditableEntity
{
    public string Nombre { get; set; } = default!;

    // Navegación
    public ICollection<Area> Areas { get; set; } = new List<Area>();
}
