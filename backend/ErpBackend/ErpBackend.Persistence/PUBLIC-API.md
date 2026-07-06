# ErpPlatform.Persistence — Public API surface (v1.0.0-rc)

Supported, semver-tracked types. Everything else (internals, expression helpers) may change.

## Base context
- `abstract class ErpDbContext : DbContext`
  - `protected ErpDbContext(DbContextOptions options)`
  - `override ConfigureConventions(...)` — applies `decimal(18,2)` default.
  - `override OnModelCreating(...)` — applies ERP conventions (soft-delete filter).
  - `protected static ApplyErpConventions(ModelBuilder)` — call from a derived `OnModelCreating`.

## Repository
- `interface IRepository<TEntity> where TEntity : BaseEntity`
  - `IQueryable<TEntity> Query()`
  - `Task<TEntity?> GetAsync(Guid id, CancellationToken = default)`
  - `Task<TEntity> AddAsync(TEntity, CancellationToken = default)`
  - `Task<TEntity> UpdateAsync(TEntity, CancellationToken = default)`
  - `Task<bool> RemoveAsync(Guid id, CancellationToken = default)`
- `class EfRepository<TEntity>(DbContext context) : IRepository<TEntity>` — `protected Context`, `protected Set`.

## Auditing
- `sealed class AuditSaveChangesInterceptor(ICurrentUserService) : SaveChangesInterceptor`

## Configuration
- `enum DatabaseProvider { Sqlite, SqlServer, PostgreSql }`
- `sealed class DatabaseOptions` — `Provider`, `ConnectionString?`, `MigrateOnStartup`; `SectionName = "Database"`; `DefaultSqliteConnection`.

## Seeding
- `interface IErpDataSeeder` — `int Order`, `Task SeedAsync(CancellationToken = default)`.

## DI / startup extensions (namespace `Microsoft.Extensions.DependencyInjection`)
- `IServiceCollection AddErpPersistence<TContext>(this IServiceCollection, IConfiguration, Action<DbContextOptionsBuilder>? configure = null) where TContext : ErpDbContext`
- `IServiceCollection AddErpSeeder<TSeeder>(this IServiceCollection) where TSeeder : class, IErpDataSeeder`
- `Task<...> ToPagedResultAsync<T>(this IQueryable<T>, PaginationParams, CancellationToken = default)`
- `Task MigrateErpDatabaseAsync<TContext>(this IServiceProvider, CancellationToken = default)`
- `Task SeedErpDataAsync(this IServiceProvider, CancellationToken = default)`
- `Task InitializeErpDatabaseAsync<TContext>(this IServiceProvider, CancellationToken = default)`

## Dependencies
- `ErpPlatform.CrossCutting` (base entities, `ICurrentUserService`, pagination, `ApplySort`).
- `Microsoft.EntityFrameworkCore` + `.Relational` + `.Sqlite` (10.0.x).
- Framework reference `Microsoft.AspNetCore.App` (for `IConfiguration`/DI/host abstractions).
