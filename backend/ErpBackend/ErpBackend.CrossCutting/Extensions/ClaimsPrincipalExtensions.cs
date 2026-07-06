using System.Security.Claims;
using ErpBackend.CrossCutting.Security;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Convenience accessors for reading standard ERP claims off a <see cref="ClaimsPrincipal"/>.
/// </summary>
public static class ClaimsPrincipalExtensions
{
    public static string? GetUserId(this ClaimsPrincipal? user) =>
        user?.FindFirst(ClaimsConstants.UserId)?.Value
        ?? user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public static string? GetEmail(this ClaimsPrincipal? user) =>
        user?.FindFirst(ClaimsConstants.Email)?.Value
        ?? user?.FindFirst(ClaimTypes.Email)?.Value;

    public static string? GetUserName(this ClaimsPrincipal? user) =>
        user?.FindFirst(ClaimsConstants.UserName)?.Value
        ?? user?.Identity?.Name;

    public static IReadOnlyList<string> GetRoles(this ClaimsPrincipal? user) =>
        user?.FindAll(ClaimsConstants.Role).Select(c => c.Value)
            .Concat(user.FindAll(ClaimTypes.Role).Select(c => c.Value))
            .Distinct()
            .ToArray()
        ?? Array.Empty<string>();

    public static IReadOnlyList<string> GetPermissions(this ClaimsPrincipal? user) =>
        user?.FindAll(ClaimsConstants.Permission).Select(c => c.Value).Distinct().ToArray()
        ?? Array.Empty<string>();

    public static bool HasPermission(this ClaimsPrincipal? user, string permission) =>
        user?.GetPermissions().Contains(permission, StringComparer.OrdinalIgnoreCase) ?? false;
}
