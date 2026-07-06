using ErpBackend.Persistence;
using ErpBackend.Persistence.Abstractions;
using ErpBackend.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Startup helpers for applying migrations and running seeders (e.g. from Program.cs).</summary>
public static class PersistenceHostExtensions
{
    /// <summary>Applies any pending migrations for <typeparamref name="TContext"/>.</summary>
    public static async Task MigrateErpDatabaseAsync<TContext>(
        this IServiceProvider services, CancellationToken cancellationToken = default)
        where TContext : ErpDbContext
    {
        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<TContext>();
        await context.Database.MigrateAsync(cancellationToken);
    }

    /// <summary>Runs every registered <see cref="IErpDataSeeder"/> in ascending order. Idempotent.</summary>
    public static async Task SeedErpDataAsync(
        this IServiceProvider services, CancellationToken cancellationToken = default)
    {
        await using var scope = services.CreateAsyncScope();
        var seeders = scope.ServiceProvider
            .GetServices<IErpDataSeeder>()
            .OrderBy(s => s.Order);

        foreach (var seeder in seeders)
        {
            await seeder.SeedAsync(cancellationToken);
        }
    }

    /// <summary>
    /// Convenience for startup/containers: applies migrations when
    /// <see cref="DatabaseOptions.MigrateOnStartup"/> is enabled, then runs all seeders.
    /// </summary>
    public static async Task InitializeErpDatabaseAsync<TContext>(
        this IServiceProvider services, CancellationToken cancellationToken = default)
        where TContext : ErpDbContext
    {
        var options = services.GetService<DatabaseOptions>();
        if (options is { MigrateOnStartup: true })
        {
            await services.MigrateErpDatabaseAsync<TContext>(cancellationToken);
        }

        await services.SeedErpDataAsync(cancellationToken);
    }
}
