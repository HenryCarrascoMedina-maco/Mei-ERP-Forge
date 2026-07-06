using ErpBackend.CrossCutting.Logging;
using ErpBackend.CrossCutting.Security;
using ErpBackend.CrossCutting.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ErpBackend.CrossCutting.Extensions;

/// <summary>
/// Registers the shared cross-cutting services into the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddErpCrossCutting(
        this IServiceCollection services,
        IConfiguration? configuration = null)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // File / export / import utilities.
        if (configuration is not null)
        {
            services.Configure<FileStorageOptions>(configuration.GetSection(FileStorageOptions.SectionName));
        }
        else
        {
            services.Configure<FileStorageOptions>(_ => { });
        }

        services.AddScoped<IFileStorageService, FileStorageService>();
        services.AddScoped<IExportService, ExportService>();
        services.AddScoped<IImportService, ImportService>();

        // Logging / audit.
        if (configuration is not null)
        {
            services.Configure<LoggingOptions>(configuration.GetSection(LoggingOptions.SectionName));
        }
        else
        {
            services.Configure<LoggingOptions>(_ => { });
        }
        services.AddScoped<IAuditService, AuditService>();

        return services;
    }
}
