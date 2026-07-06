namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Centralized permission names. Modules reference these constants instead of magic strings
/// so authorization checks stay consistent across the application.
///
/// Convention: "{module}.{action}" — e.g. "users.create".
/// </summary>
public static class PermissionConstants
{
    public static class Users
    {
        public const string View = "users.view";
        public const string Create = "users.create";
        public const string Update = "users.update";
        public const string Delete = "users.delete";
    }

    public static class Roles
    {
        public const string View = "roles.view";
        public const string Create = "roles.create";
        public const string Update = "roles.update";
        public const string Delete = "roles.delete";
    }

    public static class Catalogs
    {
        public const string View = "catalogs.view";
        public const string Manage = "catalogs.manage";
    }
}
