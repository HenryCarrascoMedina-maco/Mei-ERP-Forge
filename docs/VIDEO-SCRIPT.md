# ERP Forge — Demo Video Script (3–5 min)

> Status: design only (not yet recorded). Primary language: **English** (global developer market);
> a Spanish (LATAM) version can follow. Branding: **ERP Forge** (product) · **MeiCarOrt Software** (company).

## Brief
- **Working title:** *"ERP Forge — Build enterprise ERPs in days, not months."*
- **Audience:** developers and companies that build software (consultancies, software factories, internal teams). Technical but clear; no empty marketing.
- **Target length:** ~4:00 (with cut notes for a 3:00 tight version and a 5:00 extended version).
- **Format:** 1080p/60fps screencast + voiceover (VO) + lower-thirds + an on-screen **timer** during generation (dramatizes speed).
- **CTA (provisional):** *Try the live demo* · *Get early access*.

## Pre-production checklist
- Fresh, seeded demo DB (≈20 customers/products, roles, admin + viewer).
- Browser zoom 110–125%; terminal font 18–20pt; clean light theme.
- Example manifest ready (e.g. `Invoice.module.json`) open in the editor.
- Show **only** "ERP Forge" branding. **Hide** technical placeholder names (`@erp-platform`, `ErpPlatform.*`), `localhost:5028` ports, secrets, `.env`.
- Close notifications/IDE clutter; pre-load pages to avoid waits; trim dead time in edit.

## Flow (the commercial differentiator)
**Manifest → Generation → Backend → Frontend → CRUD → Auth → Roles → Persistence** — covered by blocks 3 → 4 → 5 → 6 → 7 → 8 → 9, closing on 10.

## Shot-by-shot script

| # | Block | Time | On screen | Voiceover | Overlay |
|---|---|---|---|---|---|
| 1 | Hook | 0:00–0:15 | ERP Forge logo → cut to a polished ERP dashboard (KPIs, tables) | "Every ERP starts the same way: weeks of building the same CRUD, auth, and permissions. What if a module took minutes?" | **ERP Forge** + tagline |
| 2 | Problem | 0:15–0:45 | Split: "the hard way" (boilerplate) vs ERP Forge | "Backend entities, DTOs, validation, REST endpoints, an Angular UI, roles, persistence — built by hand, every time. Slow, inconsistent, expensive to maintain." | Bullets: Backend · Frontend · Auth · Roles · Persistence |
| 3 | Manifest | 0:45–1:15 | Editor: `Invoice.module.json` (fields, validation, permissions, api) | "With ERP Forge you describe the module once — in a single manifest. Fields, validation, permissions, the API route. That's the only thing you write." | Highlight `fields`, `permissions`, `api` |
| 4 | Generation (the 'wow') | 1:15–1:45 | Terminal: `erpgen module --manifest Invoice.module.json --store ef` + `ng generate @erp-platform/core:module …` · **timer running** | "One manifest, two generators: a .NET backend and an Angular frontend — full-stack, in seconds. Watch the clock." | Big timer → "⏱ 00:00:08" |
| 5 | What was generated | 1:45–2:10 | File tree: entity, validated DTOs, permissions, EF repository, controller, migration / model, list, form, route + menu | "You get production-ready code: an EF Core entity, validated DTOs, permission-gated REST endpoints, and a complete Angular screen — wired into routing and the menu automatically." | Backend ✓ Frontend ✓ Migration ✓ |
| 6 | Running: CRUD | 2:10–2:50 | New module in the menu → list with data → create (validated form), edit, **422** on empty field, search, pagination | "Run it, and the module is just… there. Create, edit, delete. Server-side validation, search, pagination — all included, all consistent." | "Generated. Not hand-written." |
| 7 | Real auth | 2:50–3:20 | Logout → login screen → sign in as `admin` → dashboard | "Authentication is real and persistent — JWT, refresh tokens, hashed credentials. Not a demo stub." | — |
| 8 | Roles & permissions | 3:20–3:50 | Roles screen (permission multiselect) → adjust a role → Users screen assign role → logout → login as `viewer` → open Products → **Access denied**; action buttons hidden | "Define roles, pick permissions, assign them to users. The same permissions guard the API and hide UI actions — enterprise RBAC, out of the box." | admin vs viewer |
| 9 | Persistence | 3:50–4:05 | Create a record → restart the API/container → reload → record still there | "And it's real persistence — restart the server, your data is still there. SQLite by default, SQL Server or PostgreSQL when you scale." | "Restart → data persists ✓" |
| 10 | Close + CTA | 4:05–4:20 | ERP Forge logo + flow recap + URL | "ERP Forge. Build enterprise ERPs in days, not months. Try the live demo, and get early access today." | **demo.erpforge.dev** · "Early access →" · "by MeiCarOrt Software" |

## What to avoid showing
Placeholder names (`@erp-platform`, `ErpPlatform.*`) / raw ports; secrets / `.env` / passwords; long installs (`npm install`, restore) / waits; unintended errors / empty states / the in-memory kitchen-sink `/management/users` (use the real Security screens); internal code beyond the manifest + a glance at the generated tree; name-vs-name comparisons with Odoo/SAP (position as "vs building it by hand").

## Duration variants
- **3:00 (tight):** trim block 2 to ~15s, fold block 5 into 4, shorten 8 to "viewer → forbidden". Keep 3-4-6-9 intact (the core).
- **5:00 (extended):** add import/export + filters in block 6; live permission editing in block 8; a closing architecture beat (.NET 10 + Angular 19, multi-provider).

## Production notes
Subtle instrumental music with a build-up at block 4; burned-in subtitles; hard cuts (no long animations); record each block separately for easier re-takes; produce a 60–90s vertical (9:16) teaser from blocks 3–4–6 for LinkedIn/Shorts.
