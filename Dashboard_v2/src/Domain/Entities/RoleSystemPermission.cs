namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Asigna un permiso de sistema (clave de <see cref="Dashboard_v2.Domain.Constants.SystemPermissions"/>)
/// a un rol, de modo que todos los usuarios con ese rol hereden el permiso.
/// </summary>
public class RoleSystemPermission
{
    public int Id { get; set; }

    /// <summary>ID del rol que recibe el permiso.</summary>
    public string RoleId { get; set; } = default!;

    /// <summary>
    /// Clave del permiso de sistema (ej. "publications.view_all", "system.all").
    /// </summary>
    public string Permission { get; set; } = default!;

    /// <summary>Si el grant está activo. Permite revocar sin borrar el registro.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Quién otorgó el permiso.</summary>
    public string? GrantedBy { get; set; }

    /// <summary>Fecha/hora en que se otorgó.</summary>
    public DateTimeOffset GrantedAt { get; set; }

    // Navigation
    public Role Role { get; set; } = default!;
}
