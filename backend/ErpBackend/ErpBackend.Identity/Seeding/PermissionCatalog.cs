using ErpBackend.CrossCutting.Security;

namespace ErpBackend.Identity.Seeding;

/// <summary>
/// The platform permission keys (mirrors <see cref="PermissionConstants"/>). Used to seed the
/// Administrator role. Business-module permissions (e.g. "customers.view") are supplied by the app
/// via <c>ErpIdentityOptions.AdditionalAdminPermissions</c>, keeping this package decoupled from modules.
/// </summary>
public static class PermissionCatalog
{
    /// <summary>All platform-level permissions granted to the Administrator role.</summary>
    public static readonly string[] Platform =
    [
        PermissionConstants.Users.View, PermissionConstants.Users.Create,
        PermissionConstants.Users.Update, PermissionConstants.Users.Delete,
        PermissionConstants.Roles.View, PermissionConstants.Roles.Create,
        PermissionConstants.Roles.Update, PermissionConstants.Roles.Delete,
        PermissionConstants.Catalogs.View, PermissionConstants.Catalogs.Manage,
    ];

    /// <summary>The read-only subset (every "*.view") for the ReadOnly role.</summary>
    public static readonly string[] ReadOnly =
    [
        PermissionConstants.Users.View,
        PermissionConstants.Roles.View,
        PermissionConstants.Catalogs.View,
    ];
}
