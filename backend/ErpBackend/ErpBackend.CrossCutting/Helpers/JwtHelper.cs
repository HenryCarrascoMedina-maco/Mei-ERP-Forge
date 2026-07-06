using System.Security.Claims;
using System.Text;
using ErpBackend.CrossCutting.Security;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace ErpBackend.CrossCutting.Helpers;

/// <summary>The result of issuing an access token.</summary>
public record TokenResult(string AccessToken, DateTime ExpiresAtUtc);

/// <summary>
/// Generates signed JWT access tokens from a claim set and <see cref="JwtSettings"/>.
/// Token validation is configured by the authentication middleware.
/// </summary>
public static class JwtHelper
{
    /// <summary>Builds the symmetric signing key from the configured secret.</summary>
    public static SymmetricSecurityKey GetSigningKey(JwtSettings settings)
        => new(Encoding.UTF8.GetBytes(settings.SecretKey));

    /// <summary>Issues a signed access token containing the supplied claims.</summary>
    public static TokenResult GenerateToken(IEnumerable<Claim> claims, JwtSettings settings)
    {
        var expires = DateTime.UtcNow.AddMinutes(settings.AccessTokenExpirationMinutes);
        var credentials = new SigningCredentials(GetSigningKey(settings), SecurityAlgorithms.HmacSha256);

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = expires,
            Issuer = settings.Issuer,
            Audience = settings.Audience,
            SigningCredentials = credentials,
        };

        var token = new JsonWebTokenHandler().CreateToken(descriptor);
        return new TokenResult(token, expires);
    }

    /// <summary>Generates a cryptographically random refresh token (opaque string).</summary>
    public static string GenerateRefreshToken()
        => Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32));
}
