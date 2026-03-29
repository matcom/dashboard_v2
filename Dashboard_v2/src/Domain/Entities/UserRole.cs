using Dashboard_v2.Domain.Enums;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Tabla de unión entre <see cref="User"/> y el enum <see cref="Roles"/>.
/// Cada fila representa la asignación de exactamente un rol a un usuario.
/// El rol se guarda en la BD como entero (HasConversion&lt;int&gt;()) para mayor eficiencia.
/// </summary>
public class UserRole
{
    public string UserId { get; set; } = default!;
    public Roles Role { get; set; }

    // Navegación
    public User User { get; set; } = default!;
}
