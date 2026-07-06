using System.ComponentModel.DataAnnotations;

namespace ErpBackend.Identity.Dtos;

/// <summary>Login payload. Validated via AddErpApiValidation (-> 422 on error).</summary>
public sealed class LoginRequest
{
    [Required]
    public string UserName { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}

/// <summary>Refresh payload.</summary>
public sealed class RefreshRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>Logout payload (revokes the supplied refresh token).</summary>
public sealed class LogoutRequest
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}

/// <summary>
/// Authentication result. Additive over the original demo response (adds <see cref="RefreshToken"/>
/// and <see cref="RefreshTokenExpiresAtUtc"/>) — existing fields and the /api/auth/token contract are preserved.
/// </summary>
public sealed class AuthResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public string TokenType { get; set; } = "Bearer";

    public DateTime ExpiresAtUtc { get; set; }

    public string RefreshToken { get; set; } = string.Empty;

    public DateTime RefreshTokenExpiresAtUtc { get; set; }

    public string[] Roles { get; set; } = [];

    public string[] Permissions { get; set; } = [];
}
