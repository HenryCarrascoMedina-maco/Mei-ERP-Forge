namespace ErpBackend.CrossCutting.Security;

/// <summary>
/// JWT configuration, bound from the "Jwt" configuration section. When <see cref="SecretKey"/>
/// is empty, JWT authentication stays disabled (the template's demo endpoints remain anonymous).
/// </summary>
public class JwtSettings
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = "ErpBackend";

    public string Audience { get; set; } = "ErpBackend.Client";

    /// <summary>HMAC signing key. Must be at least 32 chars for HS256. Empty = auth disabled.</summary>
    public string SecretKey { get; set; } = string.Empty;

    public int AccessTokenExpirationMinutes { get; set; } = 60;

    public int RefreshTokenExpirationDays { get; set; } = 7;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SecretKey);
}
