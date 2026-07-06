# Authentication & Authorization Guide

> Status: introduced in **v1.0.0-rc.2**. Real, **persistent** identity (users, roles, refresh tokens)
> in the `ErpPlatform.Identity` package, built on `ErpPlatform.CrossCutting` (JWT/password/claims) and
> `ErpPlatform.Persistence` (EF Core). The existing permission-claim authorization
> (`[HasPermission]`, Angular guards, `*appHasPermission`) is unchanged — only the **source** of users
> and permissions moved from an in-memory demo to the database.

## 1. Architecture

```
ErpPlatform.CrossCutting   PasswordHelper (PBKDF2), JwtHelper, ClaimsHelper, [HasPermission], CurrentUserService
        ▲
ErpPlatform.Persistence    ErpDbContext, EfRepository, seeding pipeline
        ▲
ErpPlatform.Identity       User / Role / UserRole / RefreshToken (D3 hybrid: relational user↔role, JSON role→permissions)
   ├─ AuthService          login / refresh (rotation) / logout
   ├─ UserService, RoleService   CRUD + role assignment + permission editing
   ├─ AuthController, UsersController, RolesController   (permission-gated)
   ├─ ApplyErpIdentity(modelBuilder)   entity mappings
   ├─ AddErpIdentity(config)           DI + controllers + seeder
   └─ DefaultIdentitySeeder            admin + base roles (config-driven, idempotent)
```

**Effective permissions** = the union of the permissions of all roles assigned to a user. They are
baked into the JWT as `permission` claims at login, so authorization elsewhere needs no DB lookups.

## 2. Backend setup

```csharp
// Program.cs — after AddErpPersistence + AddErpJwtAuthentication + AddErpAuthorization
builder.Services.AddErpIdentity(builder.Configuration);

var app = builder.Build();
await app.Services.InitializeErpDatabaseAsync<AppDbContext>(); // migrate (if enabled) + seed
```
```csharp
// AppDbContext.OnModelCreating
modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
modelBuilder.ApplyErpIdentity();
base.OnModelCreating(modelBuilder);
```
Add the EF migration once: `dotnet ef migrations add AddIdentity` → tables `Users`, `Roles`,
`UserRoles`, `RefreshTokens`.

### Configuration (`Identity` section)
```jsonc
"Identity": {
  "DefaultAdmin": { "UserName": "admin", "Email": "admin@erp.local", "Password": "<from env/user-secrets>" },
  "AdditionalAdminPermissions": [ "customers.view", "customers.create", "products.view", "..." ]
}
```
- **`DefaultAdmin.Password` must come from configuration/environment** (never commit a real password).
  Production: set `Identity__DefaultAdmin__Password` as an env var / secret. The sample dev file uses a
  throwaway demo password for the zero-config demo (same policy as the dev JWT key). If no password is
  set, the admin is not seeded (a warning is logged).
- **`AdditionalAdminPermissions`** declares the app's business-module permission keys; they are added
  to the seeded Administrator role **and** form the assignable-permission catalog shown in the Roles editor.

## 3. Endpoints

| Endpoint | Auth | Purpose |
|----------|------|---------|
| `POST /api/auth/token` | anonymous | Login → access token + refresh token + roles + permissions |
| `POST /api/auth/refresh` | anonymous | Rotate refresh token → new access + refresh pair |
| `POST /api/auth/logout` | bearer | Revoke a refresh token |
| `GET  /api/auth/me` | bearer | Current identity, roles, permissions |
| `GET/POST/PUT/DELETE /api/security/users` | `users.*` | User management |
| `GET/POST/PUT/DELETE /api/security/roles` | `roles.*` | Role management |
| `GET /api/security/roles/permissions` | `roles.view` | Assignable permission catalog |

Tokens carry role + permission claims, so `[HasPermission("...")]` on any controller keeps working.

## 4. Security

- **Passwords**: PBKDF2-SHA256 via `PasswordHelper` (hash at rest, never plaintext).
- **Refresh tokens**: stored as **SHA-256 hashes** (`TokenHasher`), single-use with **rotation** — using
  a refresh token revokes it and issues a new one; a revoked/expired token returns 401.
- **Logout** revokes the refresh token server-side.
- **Soft-delete**: users are logically deleted (hidden by the global query filter); audit fields
  (`CreatedBy`/`UpdatedBy`) are stamped by the persistence interceptor.

## 5. Frontend

- **Login** (`features/system/login.component`): posts to `/api/auth/token`, then hydrates the user via
  `/api/auth/me`. Real screen — no auto-login.
- **`AuthService`** (`@erp-platform/core`): `login` / `loadCurrentUser` / `refresh` (single-flight) /
  `logout`. **`authInterceptor`** attaches the bearer token and performs **refresh-on-401** (retries the
  request; on refresh failure clears the session and redirects to `/login`). Auth endpoints are excluded
  to avoid refresh loops.
- **Guards/directives unchanged**: `authGuard` (→ `/login`), `permissionGuard`/`roleGuard` (→ `/forbidden`),
  `*appHasPermission` (hides unauthorized actions). Protected routes use `[authGuard, permissionGuard]`.
- **Management screens** (`features/security/users`, `features/security/roles`): built entirely from the
  shared generic components (`GenericTable`, `GenericFilter`, `GenericPageHeader`, the form/confirm dialogs)
  + `CrudListBase`. The role/permission pickers use the generic form's `multiselect` field type.

## 6. Seeded demo data

`DefaultIdentitySeeder` (idempotent): roles **Administrator** (all platform + additional permissions) and
**ReadOnly** (`*.view`), plus the initial **admin** user. The sample app also seeds a **viewer** (ReadOnly)
demo user so permission differences are visible. Demo credentials: `admin/Admin123!`, `viewer/Viewer123!`
(development only — replace in production).

## 7. Out of scope (this release, by design)

Public self-registration, password reset / email confirmation, 2FA, external/OAuth login, advanced
lockout, tenant management, and a DB-managed Permission/RolePermission table. The model is designed so
these are additive later (e.g. `TenantId` columns, a `Permission` table) without restructuring.

## See also
- [Persistence Guide](PERSISTENCE.md) · [Getting Started](GETTING-STARTED.md) · [Developer Guide](DEVELOPER-GUIDE.md)
- `ErpPlatform.Identity` `PUBLIC-API.md` (package surface).
