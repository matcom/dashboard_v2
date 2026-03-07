namespace Dashboard_v2.Domain.Entities;

public class User
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string PasswordHash { get; set; } = default!;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    // Navegación
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Resource> OwnedResources { get; set; } = new List<Resource>();
    public ICollection<SystemGrant> SystemGrants { get; set; } = new List<SystemGrant>();
}
