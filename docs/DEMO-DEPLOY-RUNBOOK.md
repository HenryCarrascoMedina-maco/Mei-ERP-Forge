# Demo Deployment Runbook — `demo.erpforge.dev`

> Operational, step-by-step plan to publish the ERP Forge public demo on a single VPS
> (Docker Compose + Caddy, automatic HTTPS). Companion to [DEPLOYMENT.md](DEPLOYMENT.md).
> Product: **ERP Forge** · Company: **MeiCarOrt Software**.

## 1. VPS requirements
- Ubuntu 24.04 LTS (x86_64).
- 1–2 vCPU / **2 GB RAM** / 20–40 GB SSD. (Angular build needs RAM; with 1 GB add swap — step 5.)
- Public IPv4; ports **22, 80, 443** open.
- Docker Engine + Compose plugin (installed in step 5).

## 2. Recommended provider
- **Hetzner Cloud CX22** (2 vCPU / 4 GB / ~€4.5/mo) — best price/resource. Alternatives: DigitalOcean $6, Linode/Vultr $5.
- Region: closest to the initial audience.

## 3. Domain / subdomain
- Register **`erpforge.dev`** (`.dev` forces HTTPS — ideal). Demo at **`demo.erpforge.dev`**.
- Landing lives separately on `erpforge.dev` / `www` (Netlify/Vercel/Cloudflare Pages).

## 4. DNS
```
Type  Name   Value             TTL
A     demo   <VPS_PUBLIC_IP>   300
AAAA  demo   <VPS_IPv6>        300   (optional)
```
Verify: `dig +short demo.erpforge.dev` → the VPS IP (before bringing up TLS).

## 5. One-time server setup
```bash
# (only if 1 GB RAM) 2 GB swap
fallocate -l 2G /swapfile && chmod 600 /swapfile && mkswap /swapfile && swapon /swapfile
echo '/swapfile none swap sw 0 0' >> /etc/fstab

curl -fsSL https://get.docker.com | sh                     # Docker + Compose
ufw allow OpenSSH && ufw allow 80 && ufw allow 443 && ufw --force enable
useradd -m -s /bin/bash deploy && usermod -aG docker deploy # optional deploy user
```
Get the code on the server — **A) Git** (push the private repo first, then clone with a read-only
fine-grained PAT or deploy key):
```bash
git clone https://github.com/HenryCarrascoMedina-maco/meicarort-erp-forge.git
cd meicarort-erp-forge/deploy
```
or **B) rsync** (no push):
```bash
rsync -az --exclude node_modules --exclude bin --exclude obj --exclude .git \
  <local>/erp-template/ deploy@<VPS_IP>:/home/deploy/erp-forge/
cd /home/deploy/erp-forge/deploy
```

## 6. Real environment variables (`deploy/.env` on the server — never committed)
Generate strong secrets:
```bash
openssl rand -base64 48   # Jwt__SecretKey
openssl rand -base64 18   # Identity__DefaultAdmin__Password
```
Fill `deploy/.env` from `.env.example`: `DOMAIN=demo.erpforge.dev`, real `ACME_EMAIL`,
`ASPNETCORE_ENVIRONMENT=Production`, SQLite connection, strong `Jwt__SecretKey`, a strong
`Identity__DefaultAdmin__Password` (admin = private), a public `Identity__DemoViewerPassword`
(shown on the login), the `Identity__AdditionalAdminPermissions__0..7` (customers.*/products.*),
`Demo__Seed=true`, optional `Cors__AllowedOrigins__0=https://demo.erpforge.dev`.

## 7. Deploy commands
```bash
docker compose up -d --build     # build api+web, start; Caddy auto-requests the TLS cert
docker compose ps                 # web → 80/443 ; api → 8080 (internal)
docker compose logs -f web        # watch Let's Encrypt issuance
```

## 8. Validate HTTPS
```bash
curl -sI https://demo.erpforge.dev/ | head -n 1        # 200
echo | openssl s_client -connect demo.erpforge.dev:443 -servername demo.erpforge.dev 2>/dev/null \
  | openssl x509 -noout -issuer -dates                  # Let's Encrypt, valid dates
curl -sI http://demo.erpforge.dev/ | grep -i location   # http → https redirect
```

## 9. Validate login
```bash
curl -s -X POST https://demo.erpforge.dev/api/auth/token \
  -H "Content-Type: application/json" \
  -d '{"userName":"admin","password":"<ADMIN_PASSWORD>"}'    # → accessToken
```
Browser: login `viewer/<DemoViewerPassword>` → restricted (Access denied where unpermitted); admin → full + demo data.

## 10. Reset the demo (anti-abuse)
```bash
cd deploy && docker compose down && docker volume rm deploy_erp-data && docker compose up -d
```
Cron (04:00 daily):
```
0 4 * * * cd /home/deploy/erp-forge/deploy && docker compose down && docker volume rm deploy_erp-data && docker compose up -d
```

## 11. Update the demo
```bash
git pull           # (A) or re-rsync (B)
cd deploy && docker compose up -d --build && docker image prune -f
```
Data persists in `erp-data` unless reset.

## 12. Final checklist before sharing the link
- [ ] DNS resolves to the VPS IP.
- [ ] 80/443 open; 22 restricted if possible.
- [ ] `deploy/.env` with real secrets (strong JWT key + admin password) — not dev placeholders.
- [ ] `Demo__Seed=true`; demo data present on first boot.
- [ ] Valid HTTPS (lock, Let's Encrypt, http→https redirect).
- [ ] admin + viewer login work; RBAC visible (viewer → forbidden where unpermitted).
- [ ] SPA loads at `/`; SPA routes reload (fallback); no console errors.
- [ ] Demo credentials shown on login (`viewer`); admin not public.
- [ ] Reset cron configured.
- [ ] (Optional) daily `.db` backup, analytics, rate-limit on `/api/auth/*`.
- [ ] Tested on mobile + incognito.
- [ ] **URL ready** for the landing CTA and the video close.

## Inputs needed to proceed (nothing assumed)
1. Final domain + where DNS is managed.
2. VPS provider + access (you provision it; no credentials shared with the agent).
3. Code delivery method: **GitHub private + clone** (needs your push auth + a read-only PAT/deploy key on the VPS) **or rsync** (no push).
4. If push: when you authorize `git push` and with which method (you manually, or a credential in GCM).

> Git status: rc.1/rc.2 + L1-3 commits are **local, not pushed**. rsync (B) needs no push; git (A) does.
