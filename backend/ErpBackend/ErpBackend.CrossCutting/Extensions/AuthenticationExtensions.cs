using ErpBackend.CrossCutting.Helpers;
using ErpBackend.CrossCutting.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Configures JWT bearer authentication. <b>Optional by design</b>: when no "Jwt" section with a
/// secret is configured, only the core authentication services are registered (so
/// <c>UseAuthentication</c> stays safe) and no scheme is enforced — the template's demo endpoints
/// remain anonymous.
/// </summary>
public static class AuthenticationExtensions
{
    public static IServiceCollection AddErpJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var settings = configuration.GetSection(JwtSettings.SectionName).Get<JwtSettings>();

        if (settings is null || !settings.IsConfigured)
        {
            // No secret configured → register core services only; nothing is enforced.
            services.AddAuthentication();
            return services;
        }

        services.Configure<JwtSettings>(configuration.GetSection(JwtSettings.SectionName));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = JwtHelper.GetSigningKey(settings),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };
            });

        return services;
    }
}
