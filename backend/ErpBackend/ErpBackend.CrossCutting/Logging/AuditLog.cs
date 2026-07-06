namespace ErpBackend.CrossCutting.Logging;

/// <summary>
/// Audit record describing a meaningful action performed by a user against an entity.
/// </summary>
public class AuditLog
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public DateTime OccurredAtUtc { get; set; } = DateTime.UtcNow;

    public string? UserId { get; set; }

    public string? UserName { get; set; }

    /// <summary>The action performed, e.g. "Created", "Updated", "Deleted", "LoggedIn".</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>The affected entity type, e.g. "User".</summary>
    public string? EntityName { get; set; }

    /// <summary>The affected entity identifier.</summary>
    public string? EntityId { get; set; }

    /// <summary>Optional serialized change set or extra context.</summary>
    public string? Changes { get; set; }

    public string? CorrelationId { get; set; }

    public string? IpAddress { get; set; }
}
