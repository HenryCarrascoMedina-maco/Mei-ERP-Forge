# ErpPlatform.Persistence

The opinionated, **async-first EF Core persistence layer** of the **ERP Platform**. It sits on top
of `ErpPlatform.CrossCutting` (which stays persistence-agnostic) and provides the building blocks the
code generator targets for production data access.

> Note: assembly/namespaces are `ErpBackend.Persistence.*` (a rename to `ErpPlatform.*` is a planned
> follow-up). Only the published **PackageId** is `ErpPlatform.Persistence`.

## What's inside
- **`ErpDbContext`** — base `DbContext`: global soft-delete query filter (`SoftDeleteEntity`) and a
  default `decimal(18,2)` convention. No provider-specific code.
- **`EfRepository<T>` / `IRepository<T>`** — generic async CRUD (`Query`, `GetAsync`, `AddAsync`,
  `UpdateAsync`, `RemoveAsync`). Generated per-module repositories are just a subclass.
- **`AuditSaveChangesInterceptor`** — stamps `CreatedAt/By` and `UpdatedAt/By` from
  `ICurrentUserService`, and converts physical deletes of soft-delete entities into logical ones.
- **`AddErpPersistence<TContext>(configuration, configure?)`** — registers the context, provider,
  interceptor and repositories. Provider comes from the `Database` config section.
- **`ToPagedResultAsync`** — async pagination (count + page) reusing CrossCutting's `ApplySort`.
- **Seeding** — `IErpDataSeeder` + `AddErpSeeder<T>()` + `SeedErpDataAsync()` (idempotent runner).
- **Startup** — `MigrateErpDatabaseAsync<T>()`, `InitializeErpDatabaseAsync<T>()` (migrate-on-startup
  + seed), ideal for `docker compose up`.

## Quick start
```csharp
// Program.cs
builder.Services.AddErpPersistence<AppDbContext>(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeErpDatabaseAsync<AppDbContext>(); // migrate (if enabled) + seed
```

```jsonc
// appsettings.json
"Database": { "Provider": "Sqlite", "MigrateOnStartup": true },
"ConnectionStrings": { "Default": "Data Source=erp-platform.db" }
```

```csharp
// AppDbContext.cs (in the API project, alongside generated modules)
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : ErpDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        base.OnModelCreating(modelBuilder); // applies ERP conventions
    }
}
```

## Providers
SQLite is implemented out of the box. For **SQL Server / PostgreSQL**, reference the provider package
and pass a `configure` delegate:
```csharp
builder.Services.AddErpPersistence<AppDbContext>(
    builder.Configuration,
    opt => opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
```
Migrations are provider-specific — keep a separate migration set per provider.

See `docs/PERSISTENCE.md` in the platform repo for the full guide.
