using ErpBackend.Identity.Dtos;

namespace ErpBackend.Identity.Services;

/// <summary>Real authentication backed by the database: credential login, persisted refresh-token
/// rotation, and revocation. Issues JWTs via the shared <c>JwtHelper</c> with role + permission claims,
/// so the existing permission-based authorization keeps working unchanged.</summary>
public interface IAuthService
{
    /// <summary>Validates credentials and issues an access token + a persisted refresh token.</summary>
    Task<AuthResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    /// <summary>Validates a refresh token, rotates it (revokes the old, issues a new pair).</summary>
    Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken cancellationToken = default);

    /// <summary>Revokes a refresh token (logout). Idempotent.</summary>
    Task LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
