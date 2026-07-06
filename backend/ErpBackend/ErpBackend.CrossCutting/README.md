# ErpPlatform.CrossCutting

The reusable ASP.NET Core cross-cutting layer of the **ERP Platform** — the backend counterpart of
the `@erp-platform/core` Angular package.

> Note: the assembly/namespaces are currently `ErpBackend.CrossCutting.*` (a namespace rename to
> `ErpPlatform.*` is a planned follow-up). Only the published **PackageId** is `ErpPlatform.CrossCutting`.

## What's inside
- **Responses / Pagination** — `ApiResponse<T>`, `PagedResponse<T>`, `ErrorResponse`, `PaginationParams`, `PagedResult<T>`, `QueryableExtensions`.
- **Errors** — `AppException` hierarchy + `GlobalExceptionMiddleware` (consistent error envelopes).
- **Observability** — correlation id, request/performance logging, `IAuditService`.
- **Security** — JWT auth (`AddErpJwtAuthentication`), permission authorization (`[HasPermission]`), password hashing/policy.
- **Helpers / Utilities** — string/date/file/CSV/JWT helpers; file storage; CSV export/import.

## Quick start
```csharp
using ErpBackend.CrossCutting.Extensions;

builder.Services.AddErpCrossCutting(builder.Configuration);
builder.Services.AddErpJwtAuthentication(builder.Configuration); // optional (Jwt:SecretKey)
builder.Services.AddErpAuthorization();

app.UseErpCrossCutting();   // correlation -> request log -> performance -> global exception
app.UseAuthentication();
app.UseAuthorization();
```

See `PUBLIC-API.md` for the supported public surface and stability guarantees.
