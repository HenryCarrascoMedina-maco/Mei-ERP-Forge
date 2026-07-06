using ErpBackend.CrossCutting.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Routes ASP.NET Core model-validation failures (e.g. DataAnnotations on DTOs) through the
/// platform's standard error pipeline: invalid <c>ModelState</c> becomes a <see cref="ValidationException"/>,
/// which the global exception middleware renders as a <c>ValidationErrorResponse</c> — instead of the
/// default <c>[ApiController]</c> ProblemDetails 400.
/// </summary>
public static class ApiValidationExtensions
{
    public static IServiceCollection AddErpApiValidation(this IServiceCollection services)
    {
        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.InvalidModelStateResponseFactory = context =>
            {
                var errors = context.ModelState
                    .Where(entry => entry.Value is { Errors.Count: > 0 })
                    .ToDictionary(
                        entry => entry.Key,
                        entry => entry.Value!.Errors.Select(e => e.ErrorMessage).ToArray());

                throw new ValidationException(errors);
            };
        });
        return services;
    }
}
