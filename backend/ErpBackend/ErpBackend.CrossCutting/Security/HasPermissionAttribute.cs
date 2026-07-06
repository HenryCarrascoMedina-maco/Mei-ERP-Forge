using Microsoft.AspNetCore.Authorization;

namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// Authorizes an endpoint by permission. Resolves to a dynamic policy handled by
/// <see cref="PermissionHandler"/>.
///
/// Usage: <c>[HasPermission(PermissionConstants.Users.Create)]</c>
/// </summary>
public sealed class HasPermissionAttribute : AuthorizeAttribute
{
    public const string PolicyPrefix = "PERMISSION:";

    public HasPermissionAttribute(string permission)
    {
        Permission = permission;
    }

    public string Permission
    {
        get => Policy?.Substring(PolicyPrefix.Length) ?? string.Empty;
        set => Policy = $"{PolicyPrefix}{value}";
    }
}
