using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ErpBackend.Api.Persistence;

/// <summary>
/// Design-time factory used by <c>dotnet ef</c> (migrations / database update). Builds the context
/// directly from configuration so the EF tools never have to run the full application startup.
/// Mirrors the runtime SQLite default; the connection string comes from configuration.
/// </summary>
public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var connectionString = configuration.GetConnectionString("Default")
            ?? "Data Source=erp-platform.db";

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connectionString)
            .Options;

        return new AppDbContext(options);
    }
}
