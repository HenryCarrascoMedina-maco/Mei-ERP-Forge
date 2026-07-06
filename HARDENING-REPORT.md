# Framework Hardening Report — v1.0.0-rc

Review of strict typing, nullability, error handling, validation, edge cases, security and basic
performance, with the risks to address before/after the stable v1.0.0.

## 1. Strict typing
- **Frontend**: `strict: true` plus `noImplicitOverride`, `noImplicitReturns`,
  `noFallthroughCasesInSwitch`, `noPropertyAccessFromIndexSignature`, and Angular
  `strictTemplates`/`strictInjectionParameters`/`strictInputAccessModifiers`. **Hardened further**
  this pass with `noUnusedLocals` + `noUnusedParameters` (build stays clean).
- **Backend**: `Nullable` enabled, `ImplicitUsings` on, **0 warnings**. Generics used throughout
  (`Result<T>`, `PagedResult<T>`, `ApiResponse<T>`, `ImportResult<T>`, `ExportColumn<T>`).
- **Risk**: low. No `any` leakage in shared APIs; `unknown` used where appropriate (table rows, action context).

## 2. Nullability
- Frontend models use explicit `| null` on optional contract fields; `ApiService` unwraps to `T | null`.
- Backend reference types are non-null by default; DTOs initialize strings to `string.Empty`;
  `Get` returns `T?` and controllers throw `NotFoundException` on null.
- **Risk**: low.

## 3. Error handling
- Backend: every unhandled error flows through `GlobalExceptionMiddleware` → standard
  `ErrorResponse`/`ValidationErrorResponse`; known `AppException` types map to status codes; 500s log
  with correlation id and only expose `Detail` in Development. **Verified at runtime** (404 envelope).
- Frontend: `errorInterceptor` converts failures to readable toasts via `ErrorHandlerService`, signs
  out on 401, and re-throws so callers can react. List loads set `loading=false` on error.
- **Risk**: low.

## 4. Validation
- Backend: `PaginationParams` clamps `PageSize` to ≤100 and `Page` ≥1 (prevents oversized pages);
  `PasswordPolicy` enforces strength; import validates required headers + per-row mapping with row errors.
- Frontend: reusable validators (`email`, `password`, `dateRange`, `file`, `duplicate`, `notBlank`)
  and `FormUtil.applyServerErrors` binds backend field errors onto controls.
- **Risk**: low.

## 5. Edge cases reviewed
- Empty CSV / missing headers → import returns a header/empty error rather than throwing.
- File path traversal → `FileStorageService` resolves and rejects paths outside the storage root.
- Duplicate catalog code → `ConflictException` (409).
- Pagination beyond last page → returns empty items with correct meta.
- Missing entity → `NotFoundException` (404) with a descriptive message.
- Concurrent in-memory access → `ConcurrentDictionary` in the sample repository.

## 6. Security
| Item | Status | Note |
|------|--------|------|
| Password storage | ✅ PBKDF2 (HMAC-SHA256, 100k iters, per-salt), constant-time verify | — |
| JWT | ✅ HS256, issuer/audience/lifetime validated, 30s clock skew | Optional/config-driven |
| Authorization | ✅ Dynamic permission policies (`[HasPermission]`), 401/403 verified | — |
| User enumeration | ✅ Generic "invalid username or password" | — |
| Error leakage | ✅ Stack/detail only in Development for 500s | — |
| Security headers | ⚠️ `SecurityHeadersMiddleware` from the plan not implemented | **Roadmap** |
| CORS | ⚠️ Dev policy is permissive (`AllowAnyHeader/Method`, any origin) | **Restrict origins for production** (config-driven) |
| Secret management | ⚠️ Dev `Jwt:SecretKey` is in `appsettings.Development.json` | **Use user-secrets/env/vault in prod**; production `appsettings.json` ships with no secret (auth stays off until configured) |
| Upload limits | ⚠️ Relies on ASP.NET default multipart body limit | Add an explicit max size for import/upload endpoints |

## 7. Basic performance
- Frontend: standalone + lazy routes, `OnPush` everywhere, signals, server-side pagination/sort/filter.
  Initial bundle ~514 kB (Material) — acceptable; trim per-route if needed.
- Backend: stateless services, `IQueryable` paging, slow-request detection via `PerformanceMiddleware`.
  First-request cold start (~3.5s, JIT) is expected and logged.
- **Risk**: low for typical ERP scale.

## Risks to address (priority)
1. **CORS** — make allowed origins configurable and restrict in production. *(Medium)*
2. **Security headers** — add `SecurityHeadersMiddleware` (HSTS, X-Content-Type-Options, etc.). *(Medium)*
3. **Secret management** — move `Jwt:SecretKey` to user-secrets/env; document. *(Medium)*
4. **Upload size limits** — explicit limit on import/upload endpoints. *(Low)*
5. **Automated tests** — no unit/integration tests yet; add a baseline. *(Medium)*

None of these block the **rc** tag; they are tracked on the roadmap for v1.0.0 / v1.1.0.
