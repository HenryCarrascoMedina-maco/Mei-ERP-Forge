using ErpBackend.CrossCutting.Constants;
using Microsoft.AspNetCore.Http;

namespace ErpBackend.CrossCutting.Middlewares;

/// <summary>
/// Ensures every request has a correlation id. It reads the incoming
/// <c>X-Correlation-Id</c> header or generates a new one, stores it in
/// <see cref="HttpContext.Items"/> and echoes it back on the response.
/// </summary>
public class CorrelationIdMiddleware
{
    public const string ItemsKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers[HttpConstants.CorrelationIdHeader].FirstOrDefault();

        if (string.IsNullOrWhiteSpace(correlationId))
        {
            correlationId = Guid.NewGuid().ToString("N");
        }

        context.Items[ItemsKey] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[HttpConstants.CorrelationIdHeader] = correlationId;
            return Task.CompletedTask;
        });

        await _next(context);
    }
}

/// <summary>
/// Convenience accessor for retrieving the current request's correlation id.
/// </summary>
public static class CorrelationIdAccessor
{
    public static string? GetCorrelationId(this HttpContext context) =>
        context.Items.TryGetValue(CorrelationIdMiddleware.ItemsKey, out var value)
            ? value as string
            : null;
}
