# ErpPlatform.Identity

Lightweight, **persistent authentication & authorization** for the **ERP Platform**. Real
database-backed users, roles and refresh tokens — built on `ErpPlatform.CrossCutting`
(`PasswordHelper`/`JwtHelper`/permission claims) and `ErpPlatform.Persistence`. The existing
permission-based authorization (`[HasPermission]`, guards, directives) keeps working unchanged: the
issued JWT still carries role + permission claims.

> Note: assembly/namespaces are `ErpBackend.Identity.*` (rename to `ErpPlatform.*` is a planned
> follow-up). Only the published **PackageId** is `ErpPlatform.Identity`.

## What's inside
- **Entities** — `User` (soft-delete), `Role` (`Permissions` as a JSON string collection),
  `UserRole` (relational join — auditable, multi-tenant-ready), `RefreshToken` (hashed at rest).
  (D3 hybrid: relational user↔role, JSON role→permissions.)
- **`ApplyErpIdentity(modelBuilder)`** — registers the entity mappings on your `AppDbContext`.
- **`IAuthService`** — `LoginAsync`, `RefreshAsync` (rotation), `LogoutAsync` (revoke). Effective
  permissions = union of the user's roles' permissions.
- **`AuthController`** — `POST /api/auth/token` (login, kept for back-compat), `POST /api/auth/refresh`,
  `POST /api/auth/logout`, `GET /api/auth/me`.
- **`DefaultIdentitySeeder`** (`IErpDataSeeder`) — idempotent Administrator/ReadOnly roles + initial
  admin user (credentials from configuration, never hardcoded).
- **`AddErpIdentity(configuration)`** — wires options, the auth service, the seeder and the controllers.

## Quick start
```csharp
// Program.cs (after AddErpPersistence + AddErpJwtAuthentication + AddErpAuthorization)
builder.Services.AddErpIdentity(builder.Configuration);
```
```csharp
// AppDbContext.OnModelCreating
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
modelBuilder.ApplyErpIdentity();
base.OnModelCreating(modelBuilder);
```
```jsonc
// appsettings — admin credentials come from config/env (never commit a real password)
"Identity": {
  "DefaultAdmin": { "UserName": "admin", "Email": "admin@erp.local", "Password": "<from env>" },
  "AdditionalAdminPermissions": [ "customers.view", "products.view" ]
}
```

Then add an EF migration (`dotnet ef migrations add AddIdentity`) and the seeder runs via
`InitializeErpDatabaseAsync` at startup.

## Security notes
- Passwords: PBKDF2 (via `PasswordHelper`). Refresh tokens: stored as SHA-256 hashes, rotated on use.
- Out of scope (by design, this release): public self-registration, email confirmation/reset, 2FA,
  external/OAuth login, advanced lockout, tenant management, a DB-managed Permission/RolePermission table.

See `docs/AUTHENTICATION.md` in the platform repo for the full guide.
