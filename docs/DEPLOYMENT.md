# Deployment Guide — Public Demo (Docker)

A cheap, stable, credible **public demo** of ERP Forge on a single small VPS, using Docker Compose +
Caddy (automatic HTTPS). Frontend and API are served **same-origin** (FE at `/`, API at `/api`) — no
public CORS needed.

```
Internet ──HTTPS──▶ web (Caddy: TLS + SPA + reverse_proxy /api) ──▶ api (.NET 10 :8080, internal)
                                                                      SQLite on the erp-data volume
```

## Files (`deploy/`)
- `Dockerfile.api` — multi-stage .NET 10 build → non-root runtime, SQLite under `/data`.
- `Dockerfile.web` — multi-stage Angular prod build → Caddy serving the static bundle.
- `Caddyfile` — TLS, security headers, `/api/*` → api, SPA fallback.
- `docker-compose.yml` — `web` (ports 80/443) + `api` (internal) + volumes.
- `.env.example` — copy to `.env` (gitignored) and fill secrets.

## 1. Prerequisites (VPS)
- A small Linux VPS (e.g. Hetzner CX22 ~€4.5/mo, DigitalOcean/Linode $6/mo). 1 vCPU / 2 GB is plenty.
- Docker Engine + Compose plugin.
- A DNS A record pointing your domain (e.g. `demo.erpforge.dev`) at the VPS IP (required for HTTPS).
- Inbound 80/443 open.

## 2. Configure
```bash
cd deploy
cp .env.example .env
# edit .env: set DOMAIN (your hostname), ACME_EMAIL, a strong Jwt__SecretKey,
# Identity__DefaultAdmin__Password and Identity__DemoViewerPassword.
```
Secrets live only in `.env` on the server — never committed.

## 3. Build & run
```bash
docker compose up -d --build
```
- Caddy obtains/renews a Let's Encrypt certificate for `DOMAIN` automatically.
- The API applies EF migrations on startup (`Database__MigrateOnStartup=true`) and the seeders run:
  base roles + admin + viewer, and (when `Demo__Seed=true`) demo customers/products.

Visit `https://<DOMAIN>` → sign in with the admin / viewer credentials from `.env`.

## 4. Local smoke test (no domain, plain HTTP)
```bash
cd deploy
cp .env.example .env          # set DOMAIN=:80, and any dev passwords
docker compose up -d --build
curl -s -o /dev/null -w "%{http_code}\n" http://localhost/                 # SPA → 200
curl -s -X POST http://localhost/api/auth/token -H "Content-Type: application/json" \
     -d '{"userName":"admin","password":"<your .env password>"}'           # → token
```

## 5. Update strategy
```bash
git pull                       # or pull prebuilt images from a registry
docker compose up -d --build   # rebuild changed images, recreate containers
```
The SQLite data persists in the `erp-data` volume across updates.

## 6. Reset the demo (anti-abuse)
The demo is disposable. To wipe accumulated/abused data and re-seed clean:
```bash
docker compose down
docker volume rm deploy_erp-data       # drops the SQLite database
docker compose up -d                   # migrate + re-seed fresh demo data
```
Recommended: a nightly cron running the above (or a scheduled `docker compose restart` after volume reset).

## 7. Backup
The demo is low-value (it resets), but to back up the SQLite file:
```bash
docker compose exec api sh -c "cp /data/erp.db /data/backup-$(date +%F).db"
docker cp $(docker compose ps -q api):/data/backup-YYYY-MM-DD.db ./
```
For a real (non-demo) deployment, switch to SQL Server / PostgreSQL (see PERSISTENCE.md) and use its
native dump tooling on a schedule.

## 8. Hardening applied
- TLS + HSTS + security headers (Caddy); HTTP→HTTPS automatic.
- API is **internal only** (no published ports); reached solely via Caddy.
- `UseForwardedHeaders` (correct scheme/host behind the proxy); `UseHttpsRedirection` is **dev-only**
  (TLS is terminated at Caddy).
- OpenAPI is mapped only in Development.
- Secrets (JWT key, admin/viewer passwords) come from `.env` — never committed; the dev placeholders
  are not used in Production.
- Non-root API container; SQLite stored in a volume, outside the web root.
- CORS is config-driven (`Cors__AllowedOrigins`); unnecessary for the same-origin demo.

## 9. Pending for a real VPS rollout
- Provision the VPS + DNS; install Docker; open 80/443.
- Set real secrets in `.env` (not the demo placeholders).
- (Optional) basic rate-limiting on `/api/auth/*`, a fail2ban-style guard, and the nightly reset cron.
- (Optional) push images to GHCR and deploy by tag instead of building on the host.

## Costs (real)
| Item | ~Cost |
|------|-------|
| VPS (Hetzner CX22 / DO / Linode) | $5–6 / month |
| Domain (`.dev` / `.io`) | ~$12 / year |
| TLS (Let's Encrypt via Caddy) | $0 |
| **Total** | **≈ $6–7 / month + ~$12 / year** |
