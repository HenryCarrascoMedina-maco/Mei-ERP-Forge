namespace ErpBackend.CrossCutting.Constants;

/// <summary>
/// HTTP header names and related constants shared across the application.
/// </summary>
public static class HttpConstants
{
    public const string CorrelationIdHeader = "X-Correlation-Id";

    public const string AuthorizationHeader = "Authorization";

    public const string BearerScheme = "Bearer";

    public static class ContentTypes
    {
        public const string Json = "application/json";
        public const string Excel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
        public const string Csv = "text/csv";
        public const string Pdf = "application/pdf";
        public const string OctetStream = "application/octet-stream";
    }
}
