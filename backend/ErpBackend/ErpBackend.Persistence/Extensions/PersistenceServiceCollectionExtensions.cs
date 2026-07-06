using ErpBackend.Persistence;
using ErpBackend.Persistence.Abstractions;
using ErpBackend.Persistence.Auditing;
using ErpBackend.Persistence.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace -- DI extensions live in the standard namespace.
namespace Microsoft.Extensions.DependencyInjection;

/// <summary>Registration entry points for the ERP Platform EF Core persistence layer.</summary>
public static class PersistenceServiceCollectionExtensions
{
    /// <summary>
    /// Registers <typeparamref name="TContext"/> with the configured provider, the audit interceptor,
    /// a generic <c>IRepository&lt;&gt;</c>, and exposes the context as the base <see cref="DbContext"/>
    /// (so generated <c>EfRepository&lt;T&gt;</c> instances resolve). Provider/connection come from the
    /// <c>"Database"</c> configuration section.
    /// </summary>
    /// <param name="configure">
    /// Optional provider override. When supplied it fully controls the <see cref="DbContextOptionsBuilder"/>
    /// (use it for SQL Server / PostgreSQL without this package referencing those providers). When omitted,
    /// the provider is resolved from configuration (SQLite implemented in-box).
    /// </param>
    /// <param name="services">The DI service collection.</param>
    /// <param name="configuration">App configuration (reads the <c>"Database"</c> section).</param>
    /// <typeparam name="TContext">The application's <see cref="ErpDbContext"/>-derived context.</typeparam>
    public static IServiceCollection AddErpPersistence<TContext>(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<DbContextOptionsBuilder>? configure = null)
        where TContext : ErpDbContext
    {
        var options = BindDatabaseOptions(configuration);
        services.AddSingleton(options);

        services.AddScoped<AuditSaveChangesInterceptor>();

        services.AddDbContext<TContext>((serviceProvider, builder) =>
        {
            if (configure is not null)
            {
                configure(builder);
            }
            else
            {
                ApplyConfiguredProvider(builder, options);
            }

            builder.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
        });

        // Expose the concrete context as DbContext so the open-generic EfRepository<> resolves.
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<TContext>());
        services.TryAddScoped(typeof(IRepository<>), typeof(EfRepository<>));

        return services;
    }

    /// <summary>Registers an idempotent data seeder, run by <c>SeedErpDataAsync</c> in ascending order.</summary>
    public static IServiceCollection AddErpSeeder<TSeeder>(this IServiceCollection services)
        where TSeeder : class, IErpDataSeeder
    {
        services.AddScoped<IErpDataSeeder, TSeeder>();
        return services;
    }

    private static DatabaseOptions BindDatabaseOptions(IConfiguration configuration)
    {
        var section = configuration.GetSection(DatabaseOptions.SectionName);
        var options = section.Get<DatabaseOptions>() ?? new DatabaseOptions();

        // Connection string precedence: Database:ConnectionString -> ConnectionStrings:Default -> SQLite default.
        options.ConnectionString = options.ConnectionString
            ?? configuration.GetConnectionString("Default")
            ?? (options.Provider == DatabaseProvider.Sqlite ? DatabaseOptions.DefaultSqliteConnection : null);

        return options;
    }

    private static void ApplyConfiguredProvider(DbContextOptionsBuilder builder, DatabaseOptions options)
    {
        switch (options.Provider)
        {
            case DatabaseProvider.Sqlite:
                builder.UseSqlite(options.ConnectionString ?? DatabaseOptions.DefaultSqliteConnection);
                break;

            case DatabaseProvider.SqlServer:
            case DatabaseProvider.PostgreSql:
                throw new NotSupportedException(
                    $"Provider '{options.Provider}' is design-ready but not wired in the default package. " +
                    "Either pass a 'configure' delegate to AddErpPersistence (e.g. opt => opt.UseSqlServer(conn)) " +
                    "after referencing the provider package, or install the matching ErpPlatform.Persistence companion package.");

            default:
                throw new NotSupportedException($"Unknown database provider '{options.Provider}'.");
        }
    }
}
