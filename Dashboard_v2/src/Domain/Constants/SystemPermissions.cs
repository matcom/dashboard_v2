namespace Dashboard_v2.Domain.Constants;

/// <summary>
/// Permisos de sistema: controlan acciones globales, no recursos individuales.
/// Cada constante es un string que se almacena en SystemGrant.Permission.
/// </summary>
public abstract class SystemPermissions
{
    // ——— Usuarios ———
    /// <summary>Ver la lista de usuarios</summary>
    public const string ViewUsers           = "users.view";
    /// <summary>Crear nuevos usuarios</summary>
    public const string CreateUsers         = "users.create";
    /// <summary>Editar roles y estado de usuarios</summary>
    public const string ManageUsers         = "users.manage";

    // ——— Permisos ———
    /// <summary>Ver los grants de cualquier usuario</summary>
    public const string ViewGrants          = "grants.view";
    /// <summary>Asignar permisos de sistema a otros usuarios</summary>
    public const string GrantSystemPerms    = "grants.system.grant";
    /// <summary>Asignar permisos de recurso a otros usuarios</summary>
    public const string GrantResourcePerms  = "grants.resource.grant";
    /// <summary>Revocar permisos de sistema</summary>
    public const string RevokeSystemPerms   = "grants.system.revoke";
    /// <summary>Revocar permisos de recurso</summary>
    public const string RevokeResourcePerms = "grants.resource.revoke";

    // ——— Publicaciones ———
    /// <summary>Ver todas las publicaciones (sin restricción de propietario)</summary>
    public const string ViewAllPublications    = "publications.view_all";
    /// <summary>Crear publicaciones</summary>
    public const string CreatePublications     = "publications.create";
    /// <summary>Editar cualquier publicación</summary>
    public const string EditAnyPublication     = "publications.edit_any";
    /// <summary>Eliminar cualquier publicación</summary>
    public const string DeleteAnyPublication   = "publications.delete_any";

    // ——— Sistema ———
    /// <summary>Acceso completo a todas las funciones (super-admin)</summary>
    public const string All = "system.all";

    /// <summary>
    /// Todos los permisos para asignar al administrador inicial.
    /// </summary>
    public static readonly IReadOnlyList<string> AllPermissions =
    [
        ViewUsers, CreateUsers, ManageUsers,
        ViewGrants, GrantSystemPerms, GrantResourcePerms, RevokeSystemPerms, RevokeResourcePerms,
        ViewAllPublications, CreatePublications, EditAnyPublication, DeleteAnyPublication,
        All
    ];
}
