using ErpBackend.CrossCutting.Common;

namespace ErpBackend.Identity.Entities;

/// <summary>
/// A persisted refresh token (stored as a SHA-256 hash, never in plaintext). Supports rotation:
/// on use the old token is revoked and a new one issued. A token is valid when it is not revoked
/// and not expired.
/// </summary>
public class RefreshToken : BaseEntity
{
    public Guid UserId { get; set; }

    /// <summary>SHA-256 hash (base64) of the opaque refresh token. The raw value is returned to the client once.</summary>
    public string TokenHash { get; set; } = string.Empty;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public bool IsValid(DateTime utcNow) => RevokedAtUtc is null && ExpiresAtUtc > utcNow;
}
