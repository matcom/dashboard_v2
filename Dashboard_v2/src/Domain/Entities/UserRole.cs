using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Domain.Entities;

public class UserRole
{
    public string UserId { get; set; } = default!;
    public Roles Role { get; set; }

    // Navegación
    public User User { get; set; } = default!;
}
