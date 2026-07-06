using System.Security.Claims;
using ErpBackend.CrossCutting.Security;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>
/// Builds and reads the standard ERP claim set. Used when issuing tokens (see <c>JwtHelper</c>);
/// for reading claims off the current request prefer <c>ClaimsPrincipalExtensions</c>.
/// </summary>
public static class ClaimsHelper
{
    /// <summary>
    /// Builds the claim list for a user, including a role claim per role and a permission claim
    /// per permission.
    /// </summary>
    public static IReadOnlyList<Claim> BuildClaims(
        string userId,
        string? email = null,
        string? userName = null,
        IEnumerable<string>? roles = null,
        IEnumerable<string>? permissions = null)
    {
        var claims = new List<Claim> { new(ClaimsConstants.UserId, userId) };

        if (!string.IsNullOrWhiteSpace(email))
        {
            claims.Add(new Claim(ClaimsConstants.Email, email));
        }
        if (!string.IsNullOrWhiteSpace(userName))
        {
            claims.Add(new Claim(ClaimsConstants.UserName, userName));
        }
        foreach (var role in roles ?? Enumerable.Empty<string>())
        {
            claims.Add(new Claim(ClaimsConstants.Role, role));
        }
        foreach (var permission in permissions ?? Enumerable.Empty<string>())
        {
            claims.Add(new Claim(ClaimsConstants.Permission, permission));
        }

        return claims;
    }

    public static string? GetValue(IEnumerable<Claim> claims, string type)
        => claims.FirstOrDefault(c => c.Type == type)?.Value;
}
