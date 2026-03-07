using Dashboard_v2.Domain.Common;

namespace Dashboard_v2.Domain.Entities;

/// <summary>
/// Representa un permiso de sistema asignado directamente a un usuario.
/// Los permisos de sistema controlan acciones globales (crear usuarios, ver todos los
/// recursos, asignar permisos, etc.) en lugar de permisos sobre recursos individuales.
/// </summary>
public class SystemGrant : BaseAuditableEntity
{
    /// <summary>Usuario que recibe el permiso.</summary>
    public string UserId { get; set; } = default!;

    /// <summary>
    /// Clave del permiso (ver <see cref="Dashboard_v2.Domain.Constants.SystemPermissions"/>).
    /// Ejemplos: "users.create", "publications.view_all", "system.all".
    /// </summary>
    public string Permission { get; set; } = default!;

    /// <summary>Quién otorgó el permiso.</summary>
    public string? GrantedBy { get; set; }

    /// <summary>Fecha/hora en que se otorgó.</summary>
    public DateTimeOffset GrantedAt { get; set; }

    /// <summary>Fecha de expiración opcional. Null = nunca expira.</summary>
    public DateTimeOffset? ExpiresAt { get; set; }

    /// <summary>Si el grant está activo. Permite revocar sin borrar el registro.</summary>
    public bool IsActive { get; set; } = true;

    public User User { get; set; } = default!;

    // ——— Helpers ———
    public bool IsExpired() => ExpiresAt.HasValue && ExpiresAt.Value < DateTimeOffset.UtcNow;
    public bool IsValid()   => IsActive && !IsExpired();
}
