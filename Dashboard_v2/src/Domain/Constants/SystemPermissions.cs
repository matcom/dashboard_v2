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

    // ——— Roles ———
    /// <summary>Ver la lista de roles</summary>
    public const string ViewRoles           = "roles.view";
    /// <summary>Crear nuevos roles</summary>
    public const string CreateRoles         = "roles.create";
    /// <summary>Eliminar roles existentes</summary>
    public const string DeleteRoles         = "roles.delete";
    /// <summary>Asignar/revocar permisos de sistema a roles</summary>
    public const string ManageRolePerms     = "roles.manage_perms";

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
        ViewRoles, CreateRoles, DeleteRoles, ManageRolePerms,
        ViewGrants, GrantSystemPerms, GrantResourcePerms, RevokeSystemPerms, RevokeResourcePerms,
        ViewAllPublications, CreatePublications, EditAnyPublication, DeleteAnyPublication,
        All
    ];

    /// <summary>
    /// Etiquetas legibles por módulo, usadas en la UI para mostrar los permisos agrupados.
    /// </summary>
    public static readonly IReadOnlyList<(string Key, string Label, string Module)> PermissionDescriptions =
    [
        (ViewUsers,           "Ver lista de usuarios",              "Usuarios"),
        (CreateUsers,         "Crear usuarios",                     "Usuarios"),
        (ManageUsers,         "Gestionar usuarios (roles/estado)",  "Usuarios"),

        (ViewRoles,           "Ver lista de roles",                 "Roles"),
        (CreateRoles,         "Crear roles",                        "Roles"),
        (DeleteRoles,         "Eliminar roles",                     "Roles"),
        (ManageRolePerms,     "Gestionar permisos de roles",        "Roles"),

        (ViewGrants,          "Ver permisos de cualquier usuario",  "Permisos"),
        (GrantSystemPerms,    "Asignar permisos de sistema",        "Permisos"),
        (GrantResourcePerms,  "Asignar permisos de recurso",        "Permisos"),
        (RevokeSystemPerms,   "Revocar permisos de sistema",        "Permisos"),
        (RevokeResourcePerms, "Revocar permisos de recurso",        "Permisos"),

        (ViewAllPublications, "Ver todas las publicaciones",        "Publicaciones"),
        (CreatePublications,  "Crear publicaciones",                "Publicaciones"),
        (EditAnyPublication,  "Editar cualquier publicación",       "Publicaciones"),
        (DeleteAnyPublication,"Eliminar cualquier publicación",     "Publicaciones"),

        (All,                 "Acceso total al sistema (super-admin)", "Sistema"),
    ];
}
