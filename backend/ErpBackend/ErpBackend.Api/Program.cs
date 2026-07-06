using ErpBackend.Api.Persistence;
using ErpBackend.CrossCutting.Extensions;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Shared cross-cutting services (current user, http context accessor, file/export/import, audit...).
builder.Services.AddErpCrossCutting(builder.Configuration);

// Route model-validation (DataAnnotations) errors through the standard ValidationErrorResponse.
builder.Services.AddErpApiValidation();

// EF Core persistence (provider/connection from the "Database" config section; SQLite by default).
// Generated modules use EF repositories; migrations apply + seeders run at startup via
// InitializeErpDatabaseAsync (below).
builder.Services.AddErpPersistence<AppDbContext>(builder.Configuration);

// Generated modules register their services here (idempotent insert by `erpgen`).
builder.Services.AddCustomerModule();
builder.Services.AddProductModule();
// erp-generated:modules

// JWT authentication is optional: with no "Jwt" secret configured this only registers the core
// auth services, so demo/sample endpoints stay anonymous. Permission-based authorization
// ([HasPermission("...")]) is always available.
builder.Services.AddErpJwtAuthentication(builder.Configuration);
builder.Services.AddErpAuthorization();

// Real, DB-backed identity (users/roles/refresh tokens) + AuthController + default admin/role seeding.
builder.Services.AddErpIdentity(builder.Configuration);
// Demo-only: seeds a read-only "viewer" user so permission differences are demonstrable (Api, not the package).
builder.Services.AddErpSeeder<ErpBackend.Api.Seeding.DemoIdentitySeeder>();
// Demo-only business data (Customers/Products) when Demo:Seed=true, so the public demo looks real.
builder.Services.AddErpSeeder<ErpBackend.Api.Seeding.DemoDataSeeder>();

// Behind a reverse proxy (e.g. Caddy) the app must read the real scheme/host from forwarded headers.
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

// CORS: locked to configured origins in production; permissive fallback for local dev / same-origin.
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("X-Correlation-Id");
        if (allowedOrigins.Length > 0)
        {
            policy.WithOrigins(allowedOrigins);
        }
        else
        {
            // No origins configured → permissive (dev convenience). Lock via Cors:AllowedOrigins in prod.
            policy.SetIsOriginAllowed(_ => true);
        }
    });
});

var app = builder.Build();

// Must run first: honor X-Forwarded-Proto/For from the reverse proxy (correct scheme/host downstream).
app.UseForwardedHeaders();

// Apply pending EF Core migrations (when Database:MigrateOnStartup is enabled) and run any
// registered data seeders. Idempotent and safe on every startup / container boot.
await app.Services.InitializeErpDatabaseAsync<AppDbContext>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Correlation id + global exception handling must wrap the rest of the pipeline.
app.UseErpCrossCutting();

// HTTPS redirection only in local dev; in containers TLS is terminated by the reverse proxy (Caddy),
// so the app serves plain HTTP internally and must not redirect.
if (app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseCors();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
