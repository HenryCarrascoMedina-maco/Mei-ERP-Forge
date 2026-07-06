namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Standard claim type names used across the application's JWT tokens.
/// </summary>
public static class ClaimsConstants
{
    public const string UserId = "uid";
    public const string Email = "email";
    public const string UserName = "username";
    public const string FullName = "name";
    public const string Role = "role";
    public const string Permission = "permission";
    public const string TenantId = "tenant";
}
