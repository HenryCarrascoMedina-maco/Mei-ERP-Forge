using ErpBackend.CrossCutting.Middlewares;
using Microsoft.AspNetCore.Builder;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Registers the shared cross-cutting middleware into the HTTP pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the shared observability + error middleware in the correct order: correlation id →
    /// request logging → performance → global exception handling. Call this early in the pipeline,
    /// before routing, so that every downstream request/error is traced and handled consistently.
    /// </summary>
    public static IApplicationBuilder UseErpCrossCutting(this IApplicationBuilder app)
    {
        app.UseMiddleware<CorrelationIdMiddleware>();
        app.UseMiddleware<RequestLoggingMiddleware>();
        app.UseMiddleware<PerformanceMiddleware>();
        app.UseMiddleware<GlobalExceptionMiddleware>();
        return app;
    }
}
