namespace ErpBackend.CrossCutting.Logging;

/// <summary>
/// Configuration for the request-logging and performance middleware. Bind from the
/// "ErpLogging" section (kept distinct from the framework's built-in "Logging" section).
/// </summary>
public class LoggingOptions
{
    public const string SectionName = "ErpLogging";

    /// <summary>Emit a structured log entry for every completed request.</summary>
    public bool EnableRequestLogging { get; set; } = true;

    /// <summary>Emit a warning when a request exceeds <see cref="SlowRequestThresholdMs"/>.</summary>
    public bool EnablePerformanceLogging { get; set; } = true;

    /// <summary>Threshold (ms) above which a request is logged as slow.</summary>
    public int SlowRequestThresholdMs { get; set; } = 1000;
}
