# ERP Framework Roadmap

Planned evolution after freezing **v1.0.0-rc**. Dates are indicative; scope is the contract.

---

## v1.0.0 — Stable foundation (promote the rc)
**Goal:** turn the validated rc into a production-grade base by closing the integration gaps the
samples stubbed out. No new framework concepts — make the existing ones real.

- **Persistence layer**: `ErpBackend.Infrastructure` with EF Core, a generic repository + Unit of
  Work, `DbContext`, migrations; swap the in-memory sample stores behind repository interfaces.
- **Real authentication**: a login component that calls `POST /api/auth/token`, stores the JWT via
  `AuthService`, and populates `PermissionService` from `/api/auth/me`; remove the demo seed.
- **Security hardening**: configurable CORS (restrict origins), `SecurityHeadersMiddleware`,
  `Jwt:SecretKey` moved to user-secrets/env, explicit upload size limits.
- **Audit trail**: DB-backed `IAuditService` implementation (queryable history).
- **Baseline tests**: backend unit tests (helpers, `QueryableExtensions`, import/export) + a few
  frontend specs for the core shared components; wire into CI.
- **Docs**: deployment guide + environment configuration.

## v1.1.0 — Productivity & reporting
**Goal:** make common ERP tasks faster and richer.

- **Excel & PDF export** implemented behind the existing `IExportService` / `PdfHelper` interfaces
  (ClosedXML/EPPlus, QuestPDF).
- **Refresh-token** rotation + revocation; "remember me"; token auto-refresh interceptor.
- **Server-driven filters** metadata so `generic-filter` fields can be defined by the backend.
- **Bulk actions** on `generic-table` (multi-select → batch delete/activate/export).
- **Notifications** service + toast center; **i18n** (Spanish/English) scaffolding.
- **Theme service** wired to `THEME_CONFIG` (light/dark switch persisted).

## v1.2.0 — Scale & UX
**Goal:** support larger apps and richer screens.

- **App shell**: responsive sidebar/topbar layout driven by `MENU_CONFIG` (permission-filtered).
- **Virtual scrolling / large datasets** option for `generic-table`; saved views/filters.
- **Advanced form fields**: autocomplete with async data, repeatable sections, wizard/stepper forms.
- **Caching** layer (HTTP cache + server response cache) and optimistic updates.
- **Audit viewer** module + activity timeline component.
- **Accessibility & performance** pass (a11y audit, route-level bundle trimming).

## v2.0.0 — Platform
**Goal:** multi-tenant, extensible platform (may include breaking changes).

- **Multi-tenancy** (tenant resolution, per-tenant data isolation, tenant-scoped permissions).
- **Modular/plugin architecture**: feature modules as independently shippable packages.
- **Real-time** (SignalR) for live dashboards/notifications.
- **Workflow/approvals** engine and configurable business rules.
- **Microservice-ready** option (split API, gateway, message bus) and containerization/Helm charts.
- Possible **Angular/.NET major upgrades** and API contract v2 (versioned endpoints).

---

### Guiding rules across versions
- Keep `shared` / `CrossCutting` the single source of reuse; modules stay configuration-driven.
- No duplicated logic; every endpoint uses the standard envelopes; every error flows through the
  global handler. New cross-cutting concerns are added as shared services/middleware, never copied.
