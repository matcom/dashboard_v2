namespace Dashboard_v2.Domain.Entities;

public class UserRole
{
    public string UserId { get; set; } = default!;
    public string RoleId { get; set; } = default!;

    // Navegación
    public User User { get; set; } = default!;
    public Role Role { get; set; } = default!;
}
