using System.Text.Json;
using ErpBackend.CrossCutting.Middlewares;
using ErpBackend.CrossCutting.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace ErpBackend.CrossCutting.Logging;

/// <summary>
/// Default <see cref="IAuditService"/> that writes audit entries as structured logs and enriches
/// them with the current user, correlation id and client IP. Swap for a DB-backed implementation
/// to persist a queryable audit trail.
/// </summary>
public class AuditService : IAuditService
{
    private readonly ILogger<AuditService> _logger;
    private readonly ICurrentUserService _currentUser;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public AuditService(
        ILogger<AuditService> logger,
        ICurrentUserService currentUser,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _currentUser = currentUser;
        _httpContextAccessor = httpContextAccessor;
    }

    public Task LogAsync(AuditLog entry, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation(
            "{Event}: {Action} on {Entity}#{EntityId} by {UserId} (corr: {CorrelationId})",
            LogConstants.Events.AuditEntry,
            entry.Action,
            entry.EntityName,
            entry.EntityId,
            entry.UserId,
            entry.CorrelationId);

        return Task.CompletedTask;
    }

    public Task LogAsync(
        string action,
        string? entityName = null,
        string? entityId = null,
        object? changes = null,
        CancellationToken cancellationToken = default)
    {
        var context = _httpContextAccessor.HttpContext;
        var entry = new AuditLog
        {
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Changes = changes is null ? null : JsonSerializer.Serialize(changes),
            UserId = _currentUser.UserId,
            UserName = _currentUser.UserName,
            CorrelationId = context?.GetCorrelationId(),
            IpAddress = context?.Connection.RemoteIpAddress?.ToString(),
        };
        return LogAsync(entry, cancellationToken);
    }
}
