# ERP Forge — Landing Page (design + copy + conversion strategy)

> Status: design/copy only (not implemented). Product: **ERP Forge** · Company: **MeiCarOrt Software** ·
> Primary language: **English** · Market: developer-first (B2D).
> Positioning: *The platform to build enterprise ERPs on .NET + Angular.* We compete against building
> ERPs by hand — **not** against Odoo / SAP / ERPNext. CTAs: **Try the live demo** · **Get early access**.

## 1. Page structure (scroll order)
1. Nav (logo · Features · How it works · Pricing · Demo · GitHub · **Get early access**)
2. Hero (headline + subhead + 2 CTAs + hero GIF/video)
3. Trust strip (stack + open-source components — honest, no fake logos)
4. Problem
5. Solution / How it works (manifest → full-stack, 3 steps)
6. Features (grid of 6)
7. Comparison (traditional vs ERP Forge)
8. Live demo (embed/CTA + demo credentials)
9. Pricing (Community / Professional / Enterprise + Founder)
10. FAQ
11. Final CTA (early access)
12. Footer

## 2. Primary copy (English)

**Hero**
- H1: **Build enterprise ERPs in days, not months.**
- Sub: *ERP Forge turns a single manifest into a full-stack module — .NET backend, Angular UI, validation, REST API, roles and persistence. Production-ready, on the stack you already know.*
- CTAs: **Try the live demo** · **Get early access**
- Micro-trust: *.NET 10 · Angular 19 · open-source components · self-hosted*

**Problem** — *Every ERP module is the same plumbing — built by hand, again and again.* Entities, DTOs, validation, REST endpoints, an Angular screen, auth, roles, persistence. Weeks of repetitive work per module, inconsistent, painful to maintain.

**How it works — One manifest. Full stack. Minutes.**
1. **Describe** — define the module once in a JSON manifest: fields, validation, permissions, API route.
2. **Generate** — one command generates the .NET backend and the Angular frontend, wired into routing, menu and database.
3. **Run** — CRUD, search, pagination, validation, RBAC and persistence — working and consistent.

**Features (grid)**
- Manifest-driven generator — one source of truth → backend + frontend.
- .NET 10 + Angular 19 — your stack, no lock-in, self-hosted.
- Real auth & RBAC — JWT, refresh tokens, users, roles, permissions.
- Real persistence — EF Core, SQLite to SQL Server / PostgreSQL.
- Reusable component library — tables, forms, dialogs, filters.
- Regeneration-safe — change the manifest, regenerate; your edits stay.

**Final CTA** — *Stop rebuilding the same CRUD.* Try the live demo, then get early access.

## 3. Spanish summary (LATAM, later)
- H1: **Construye ERPs empresariales en días, no en meses.**
- Sub: *ERP Forge convierte un único manifest en un módulo full-stack: backend .NET, UI Angular, validación, API REST, roles y persistencia.*
- CTAs: **Probar la demo** / **Solicitar early access**
- Keep English live first; publish ES when there's LATAM traction.

## 4–5. Sections & CTAs
Primary CTA *Try the live demo* (Hero/Demo/Final) → `demo.erpforge.dev`. Conversion CTA *Get early access* → form. Tertiary *View on GitHub* (when Community repo exists). Max 2 CTAs per viewport.

## 6. Pricing (reference)
Open-core: Community free for adoption; value (full generator + identity + support) in Professional/Enterprise.

| | Community | Professional | Enterprise |
|---|---|---|---|
| Price | Free | **Founder USD 249–399 / dev / year** | Custom |
| Generator | basic (single entity, no relations) | full (EF, validation, async, multi-provider) | full + extensions |
| Angular components | ✓ | ✓ | ✓ |
| Identity (auth/roles) | — | ✓ | ✓ + SSO/white-label (roadmap) |
| Support | community | email | priority + SLA |
| License | source-available | commercial | commercial |

Founder pricing for the first cohort (honest urgency). Buttons: Community → *Get started (GitHub)* · Pro → *Get early access* · Enterprise → *Contact us*.

## 7. FAQ
What is it · Is it no-code? (no — a code generator for devs) · Do I own the code? (yes) · Database? (SQLite → SQL Server/PostgreSQL) · Production-ready? (release candidate; core working) · Use my existing app? (npm/NuGet packages) · Free vs paid? · Is there a demo? (yes, admin/viewer) · Roadmap? (public demo, more generators, AI-assisted manifest, marketplace — **future**).

## 8. Comparison (traditional vs ERP Forge)
| | Traditional hand-coding | ERP Forge |
|---|---|---|
| New CRUD module | days–weeks | minutes |
| BE/FE consistency | varies | guaranteed (one manifest) |
| Auth + RBAC | from scratch | included |
| Validation/pagination/search | re-implemented | generated |
| Maintenance/changes | manual everywhere | change manifest → regenerate |
| Stack lock-in | — | none (self-hosted) |
Header: *"Same stack. Same control. A fraction of the boilerplate."* (No unbacked "10x".)

## 9. Screenshots / GIFs
1. **Hero GIF:** manifest → command → full-stack module appears (with timer). 2. CRUD screen (search/pagination). 3. RBAC admin vs viewer (Access denied). 4. Roles editor (permission multiselect). 5. Dashboard (KPIs). 6. Generation terminal (large font). Optimized WebP/MP4 <1–2 MB, clean browser frame, no sensitive/technical-placeholder names.

## 10. Early-access form
Minimal friction: email only + *Get early access*; optional progressive fields (name, company, role). Use Tally/Formspree/ConvertKit/Notion form or a small endpoint; double opt-in + thank-you page; consent checkbox + privacy link. Separate *Contact us* (email/Calendly) for Enterprise.

## 11. Avoid (no hype)
No fabricated "10x"/"99.9% uptime"/"trusted by N"/fake logos/testimonials. Don't present roadmap features (AI, marketplace, multi-tenancy, SSO, white-label) as available — label **Roadmap**. No feature-parity claims vs SAP/Odoo. No SLA/compliance promises you can't meet. Use demonstrable claims (live demo, open-source components, self-hosted, the video timer). Label status honestly: **Release Candidate / Early access**.

## 12. Assets needed
Logo (SVG light/dark), favicon, OG image (1200×630); hero GIF/MP4 + 4–6 screenshots + embedded demo video; **deployed live demo** (L1-3 on a VPS with domain); copy + comparison table + pricing cards + feature icons; early-access form + thank-you + minimal privacy/terms; domain (`erpforge.dev`/`.io`) + static hosting (Netlify/Vercel/Cloudflare Pages); privacy-friendly analytics (Plausible/Umami).

## Conversion strategy
Golden path: Hero → *Try the live demo* → (returns impressed) → *Get early access*. The **public demo** is the engine, the **video** the hook, the **landing** the capture. One conversion goal: the early-access email. Above-the-fold communicates value in <5s. Measure: % reaching demo, % returning, % leaving email; iterate the headline if the demo converts but the email doesn't.

**Critical dependency:** *Try the live demo* needs the deployed public demo (L1-3 on VPS + domain); the video needs the landing URL. Order: landing copy → deployed demo → record video.
