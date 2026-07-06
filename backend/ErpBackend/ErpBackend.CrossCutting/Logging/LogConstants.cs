namespace ErpBackend.CrossCutting.Logging;

/// <summary>Centralized logging event names and structured-log property keys.</summary>
public static class LogConstants
{
    public static class Events
    {
        public const string RequestCompleted = "RequestCompleted";
        public const string SlowRequest = "SlowRequest";
        public const string AuditEntry = "AuditEntry";
        public const string UnhandledException = "UnhandledException";
    }

    public static class Properties
    {
        public const string CorrelationId = "CorrelationId";
        public const string UserId = "UserId";
        public const string Method = "Method";
        public const string Path = "Path";
        public const string StatusCode = "StatusCode";
        public const string ElapsedMs = "ElapsedMs";
        public const string Action = "Action";
        public const string Entity = "Entity";
    }
}
