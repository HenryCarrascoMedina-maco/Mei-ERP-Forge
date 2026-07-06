# Public API — ErpPlatform.CrossCutting (v1.0.0-rc)

Defines the **supported public surface** of the NuGet package and its stability guarantees. Types
outside this list are implementation details and may change without a major version bump.

> Namespaces are `ErpBackend.CrossCutting.*` today (PackageId `ErpPlatform.CrossCutting`). A namespace
> rename to `ErpPlatform.*` is a planned, separate follow-up and will be a documented breaking change.

## Public & supported

| Namespace | Public contracts |
|-----------|------------------|
| `…Common` | `BaseEntity`, `AuditableEntity`, `SoftDeleteEntity`, `BaseDto`, `BaseCreateDto`, `BaseUpdateDto`, `BaseCatalogDto`, `Result`, `Result<T>` |
| `…Responses` | `ApiResponse`, `ApiResponse<T>`, `ErrorResponse`, `ValidationErrorResponse`, `PagedResponse<T>`, `CreatedResponse<TKey>`, `NoContentResponse` |
| `…Pagination` | `PaginationParams`, `PagedResult<T>`, `SortParams`, `SortDirection`, `FilterParams`, `PaginationMeta` |
| `…Exceptions` | `AppException`, `NotFoundException`, `ValidationException`, `UnauthorizedException`, `ForbiddenException`, `ConflictException`, `BusinessException` |
| `…Extensions` | `AddErpCrossCutting`, `UseErpCrossCutting`, `AddErpJwtAuthentication`, `AddErpAuthorization`, `QueryableExtensions` (`ApplySort`/`ToPagedResult`/`ApplyPagination`), `ClaimsPrincipalExtensions` |
| `…Security` | `ICurrentUserService`, `JwtSettings`, `PasswordPolicy`, `PermissionConstants`, `RoleConstants`, `ClaimsConstants`, `[HasPermission]`, `PermissionRequirement` |
| `…Logging` | `IAuditService`, `AuditLog`, `LogEvent`, `LogConstants`, `LoggingOptions` |
| `…Helpers` | `StringHelper`, `DateTimeHelper`, `PasswordHelper`, `JwtHelper` (+ `TokenResult`), `ClaimsHelper`, `CsvHelper`, `FileHelper`, `PdfHelper` |
| `…Utilities` | `IFileStorageService` (+ `FileStorageOptions`, `StoredFileInfo`), `IExportService` (+ `ExportResult`, `ExportColumn<T>`), `IImportService` (+ `ImportResult<T>`, `ImportRowError`) |
| `…Constants` | `HttpConstants`, `ErrorMessages`, `SuccessMessages` |

**Guarantee:** within `1.x`, the above signatures evolve additively only (no removals/renames).

## Internal / not guaranteed
- Default service **implementations** (`CurrentUserService`, `AuditService`, `FileStorageService`,
  `ExportService`, `ImportService`, `PermissionHandler`, `PermissionPolicyProvider`) — consume via
  their interfaces / DI registration; concrete classes may change. Replace them through DI as needed.
- Middleware classes (`GlobalExceptionMiddleware`, `CorrelationIdMiddleware`, `RequestLoggingMiddleware`,
  `PerformanceMiddleware`) — register via `UseErpCrossCutting()`; not meant to be instantiated directly.
- Anything marked `internal`, and any member without XML documentation.

## Consume
```bash
dotnet add package ErpPlatform.CrossCutting
```
Then wire it as shown in `README.md`. Requires a Web SDK project (`Microsoft.NET.Sdk.Web`) because the
package uses the ASP.NET Core shared framework.
