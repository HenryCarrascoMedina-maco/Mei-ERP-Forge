namespace ErpBackend.CrossCutting.Logging;

/// <summary>
/// Records audit entries for important actions. The default implementation writes structured
/// logs; replace it with a persistence-backed implementation to store an audit trail.
/// </summary>
public interface IAuditService
{
    /// <summary>Records a fully-populated audit entry.</summary>
    Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default);

    /// <summary>
    /// Convenience overload: builds the entry, enriching it with the current user, correlation id
    /// and client IP from the request context.
    /// </summary>
    Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        object? changes = null,
        CancellationToken cancellationToken = default);
}
