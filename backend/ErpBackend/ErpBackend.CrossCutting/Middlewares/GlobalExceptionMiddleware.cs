using System.Net;
using System.Text.Json;
using ErpBackend.CrossCutting.Constants;
using ErpBackend.CrossCutting.Exceptions;
using ErpBackend.CrossCutting.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ErpBackend.CrossCutting.Middlewares;

/// <summary>
/// Catches unhandled exceptions and converts them into a consistent <see cref="ErrorResponse"/>
/// (or <see cref="ValidationErrorResponse"/>) payload. Known <see cref="AppException"/> types map to
/// their declared status codes; everything else becomes a 500 with a generic message.
/// </summary>
public class GlobalExceptionMiddleware
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.GetCorrelationId();

        ErrorResponse response = exception switch
        {
            ValidationException validation => new ValidationErrorResponse
            {
                Errors = validation.Errors
            },
            _ => new ErrorResponse()
        };

        var statusCode = exception is AppException appException
            ? (int)appException.StatusCode
            : (int)HttpStatusCode.InternalServerError;

        response.StatusCode = statusCode;
        response.CorrelationId = correlationId;
        response.ErrorCode = (exception as AppException)?.ErrorCode;
        response.Message = exception is AppException
            ? exception.Message
            : ErrorMessages.Unexpected;

        if (statusCode >= 500)
        {
            _logger.LogError(exception,
                "Unhandled exception. CorrelationId: {CorrelationId}", correlationId);
        }
        else
        {
            _logger.LogWarning(
                "Handled application exception ({StatusCode}). CorrelationId: {CorrelationId}. {Message}",
                statusCode, correlationId, exception.Message);
        }

        if (_environment.IsDevelopment() && statusCode >= 500)
        {
            response.Detail = exception.ToString();
        }

        context.Response.Clear();
        context.Response.ContentType = HttpConstants.ContentTypes.Json;
        context.Response.StatusCode = statusCode;

        await context.Response.WriteAsync(JsonSerializer.Serialize(response, JsonOptions));
    }
}
