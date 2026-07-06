# ErpPlatform.Identity — Public API surface (v1.0.0-rc)

## Entities (namespace `ErpBackend.Identity.Entities`)
- `class User : SoftDeleteEntity` — `UserName`, `Email`, `PasswordHash`, `DisplayName?`, `ICollection<UserRole> UserRoles`.
- `class Role : AuditableEntity` — `Name`, `Description?`, `List<string> Permissions`, `ICollection<UserRole> UserRoles`.
- `class UserRole : AuditableEntity` — `UserId`, `RoleId`, `User?`, `Role?`.
- `class RefreshToken : BaseEntity` — `UserId`, `TokenHash`, `ExpiresAtUtc`, `RevokedAtUtc?`, `bool IsValid(DateTime)`.

## EF mapping
- `ModelBuilder ApplyErpIdentity(this ModelBuilder)` — registers all identity configurations.

## Services (namespace `ErpBackend.Identity.Services`)
- `interface IAuthService`
  - `Task<AuthResponse> LoginAsync(LoginRequest, CancellationToken = default)`
  - `Task<AuthResponse> RefreshAsync(string refreshToken, CancellationToken = default)`
  - `Task LogoutAsync(string refreshToken, CancellationToken = default)`
- `sealed class AuthService(DbContext, IOptions<JwtSettings>) : IAuthService`

## DTOs (namespace `ErpBackend.Identity.Dtos`)
- `LoginRequest { [Required] UserName; [Required] Password }`
- `RefreshRequest { [Required] RefreshToken }`
- `LogoutRequest { [Required] RefreshToken }`
- `AuthResponse { AccessToken; TokenType; ExpiresAtUtc; RefreshToken; RefreshTokenExpiresAtUtc; Roles[]; Permissions[] }`

## Controller (namespace `ErpBackend.Identity.Controllers`)
- `AuthController` — `POST api/auth/token` (anon), `POST api/auth/refresh` (anon),
  `POST api/auth/logout` (auth), `GET api/auth/me` (auth).

## Configuration (namespace `ErpBackend.Identity.Configuration`)
- `sealed class ErpIdentityOptions` — `DefaultAdmin`, `AdditionalAdminPermissions[]`; `SectionName = "Identity"`.
- `sealed class DefaultAdminOptions` — `UserName`, `Email`, `Password`.

## Seeding (namespace `ErpBackend.Identity.Seeding`)
- `sealed class DefaultIdentitySeeder : IErpDataSeeder` (Order 0).
- `static class PermissionCatalog` — `Platform[]`, `ReadOnly[]`.

## Security (namespace `ErpBackend.Identity.Security`)
- `static class TokenHasher` — `string Hash(string token)` (SHA-256, base64).

## DI (namespace `Microsoft.Extensions.DependencyInjection`)
- `IServiceCollection AddErpIdentity(this IServiceCollection, IConfiguration)`

## Dependencies
- `ErpPlatform.CrossCutting`, `ErpPlatform.Persistence`, `Microsoft.EntityFrameworkCore` (10.0.x),
  framework reference `Microsoft.AspNetCore.App`.
