# Launch Checklist — MeiCarOrt ERP Forge / ERP Platform Starter Kit v1.0.0-rc.0

Go-to-market readiness for the release candidate. Checked items are **done in this RC**; unchecked
items are the path to GA / pilot. Grouped by area.

---

## 1. Técnico

- [x] All builds clean (frontend app, library, schematics, backend, CLI, NuGet, template pack) — 0/0.
- [x] Manifest-driven generator works full-stack (frontend schematic + `erpgen` backend tool).
- [x] Regeneration-safe: `*.generated`/`*.Generated` overwritten, editable files created once, idempotent patch points.
- [x] Two sample modules (Customer, Product) generated full-stack and runtime-verified (CRUD/permisos/validaciones).
- [x] Version frozen and consistent at `1.0.0-rc.0` (see [RELEASE-CHECKLIST.md](RELEASE-CHECKLIST.md) §4).
- [ ] Automated test suite (unit + integration) — **deferred**.
- [ ] EF Core persistence option (sample modules are in-memory) — **deferred**.
- [ ] Security hardening: `SecurityHeadersMiddleware`, tightened CORS, secret management, refresh tokens — **deferred**.
- [ ] CI pipeline running pack + `verify-local-install` as a gate — **scaffold in [docs/PUBLISHING.md](docs/PUBLISHING.md) §7**.

## 2. Documentación

- [x] [README.md](README.md) — overview + docs index.
- [x] [docs/GETTING-STARTED.md](docs/GETTING-STARTED.md) — first module in ~15 min.
- [x] [docs/DEVELOPER-GUIDE.md](docs/DEVELOPER-GUIDE.md) — structure + 10 how-tos.
- [x] [docs/COMMERCIAL-INSTALL-GUIDE.md](docs/COMMERCIAL-INSTALL-GUIDE.md) — customer-facing install.
- [x] [docs/PUBLISHING.md](docs/PUBLISHING.md) — private registry pack/publish/verify.
- [x] [docs/PACKAGING.md](docs/PACKAGING.md) · [docs/MIGRATION-GUIDE.md](docs/MIGRATION-GUIDE.md) · [docs/COMMERCIAL-DEMO.md](docs/COMMERCIAL-DEMO.md).
- [x] [RELEASE-NOTES.md](RELEASE-NOTES.md) + [RELEASE-CHECKLIST.md](RELEASE-CHECKLIST.md) + [CHANGELOG.md](CHANGELOG.md).
- [x] [PUBLIC-API.md](backend/ErpBackend/ErpBackend.CrossCutting/PUBLIC-API.md) — supported backend surface.
- [x] Backend [MANIFEST-CONTRACT.md] + manifest JSON Schema (frozen v1.0).
- [ ] Public product landing / sales one-pager — **commercial, deferred**.

## 3. Comercial

- [x] Commercial naming documented (MeiCarOrt ERP Forge ↔ technical `@erp-platform/*` / `ErpPlatform.*`).
- [x] Value proposition captured in README + Commercial Install Guide.
- [ ] Pricing / packaging tiers — **owner: business**.
- [ ] Pilot customer identified and scope agreed — **owner: business**.
- [ ] Package rename to commercial namespace (`@meicarort/*`, `MeiCarOrt.*`) — **deferred to GA** (do NOT rename before RC sign-off).

## 4. Soporte

- [x] Known limitations published ([RELEASE-NOTES.md](RELEASE-NOTES.md) §Known limitations).
- [x] Migration guide for adopting apps ([docs/MIGRATION-GUIDE.md](docs/MIGRATION-GUIDE.md)).
- [ ] Support channel / SLA defined (email, ticketing) — **owner: business**.
- [ ] Issue intake + triage process — **owner: business**.
- [ ] Versioning & deprecation policy published (SemVer; `rc.N` cadence noted in RELEASE-NOTES).

## 5. Licencias

- [x] `LICENSE.txt` shipped inside `ErpPlatform.CrossCutting` NuGet (`PackageLicenseFile`).
- [ ] Top-level repository LICENSE for the platform as a product — **owner: business / legal**.
- [ ] Commercial license terms / EULA for pilot — **owner: business / legal**.
- [ ] Third-party dependency license review (Angular/Material, ASP.NET, ajv, JwtBearer) — **owner: legal**.

## 6. Publicación privada

- [x] `.npmrc.example` + `nuget.config.example` committed (no secrets).
- [x] `.gitignore` excludes secrets, tokens, and publish artifacts.
- [x] `publishing/` scripts: `pack-all`, `publish-npm`, `publish-nuget`, `verify-local-install`.
- [x] Offline client install validated end-to-end.
- [ ] Private npm registry provisioned + `ERP_NPM_TOKEN` set → `./publishing/publish-npm.ps1`.
- [ ] Private NuGet feed provisioned + `ERP_NUGET_FEED`/`ERP_NUGET_KEY` set → `./publishing/publish-nuget.ps1`.
- [ ] **Tag the release** (see below).

### Tagging `v1.0.0-rc.0`

If/when the deliverable lives in a git repository:

```bash
git add .
git commit -m "Release candidate v1.0.0-rc.0"
git tag -a v1.0.0-rc.0 -m "MeiCarOrt ERP Forge / ERP Platform Starter Kit v1.0.0-rc.0"
# git push && git push --tags   # to your PRIVATE remote only
```

> **Freeze policy:** after tagging, `1.0.0-rc.0` is immutable. Any change → new tag `v1.0.0-rc.1`.

---

## RC sign-off

- Technical verification: **PASS** (see [RELEASE-CHECKLIST.md](RELEASE-CHECKLIST.md)).
- Ready for: **final review + pilot sale**.
- Not ready for: public/general availability (rename, persistence, tests, hardening pending).
