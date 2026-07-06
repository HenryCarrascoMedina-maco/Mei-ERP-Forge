namespace ErpBackend.CrossCutting.Logging;

/// <summary>
/// A structured log event model. Useful when forwarding events to a sink that expects a uniform
/// shape (the default middleware logs via <c>ILogger</c> directly).
/// </summary>
public class LogEvent
{
    public string Name { get; set; } = string.Empty;

    public string? Message { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;

    public string? CorrelationId { get; set; }

    public IDictionary<string, object?> Properties { get; } = new Dictionary<string, object?>();

    public LogEvent() { }

    public LogEvent(string name, string? message = null)
    {
        Name = name;
        Message = message;
    }

    public LogEvent With(string key, object? value)
    {
        Properties[key] = value;
        return this;
    }
}
