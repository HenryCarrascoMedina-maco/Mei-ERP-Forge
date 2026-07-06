namespace ErpBackend.Identity.Configuration;

/// <summary>
/// Identity configuration, bound from the <c>"Identity"</c> section. The default-admin credentials
/// come from configuration / environment (never hardcoded; see D6) and are used only to seed the
/// first administrator when the database has no users yet.
/// </summary>
public sealed class ErpIdentityOptions
{
    public const string SectionName = "Identity";

    public DefaultAdminOptions DefaultAdmin { get; set; } = new();

    /// <summary>
    /// Extra permission keys granted to the seeded Administrator role beyond the platform catalog
    /// (e.g. permissions for generated business modules like "customers.view"). Supplied by the app
    /// so the Identity package stays decoupled from specific modules.
    /// </summary>
    public string[] AdditionalAdminPermissions { get; set; } = [];
}

/// <summary>Credentials for the seeded initial administrator.</summary>
public sealed class DefaultAdminOptions
{
    public string UserName { get; set; } = "admin";

    public string Email { get; set; } = "admin@erp.local";

    /// <summary>Initial password. MUST be provided via configuration/environment in any real
    /// deployment; empty disables admin seeding (logged as a warning).</summary>
    public string Password { get; set; } = string.Empty;
}
