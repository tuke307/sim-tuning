---
applyTo: '**'
description: 'Project orientation for sim-tuning — what the product is, monorepo map, toolchain, commands, local docker environment, deployment, and testing'
---

# Project Orientation — SimTuning

## What this is

SimTuning (`sim-tuning`) is a two-stroke engine simulation and dyno tuning app: engine geometry (displacement, compression, port timing diagram), intake resonance and carburettor sizing, exhaust (expansion chamber) design, and a dyno that derives power curves from recorded engine sound (spectrogram → RPM points → DBSCAN clusters) and GPS speed / coast-down runs. Offline-first PWA plus a native iOS app.

`v3` is a full rewrite of the former .NET MAUI app (branch `v2.0.0`). The migration plan lives in `docs/v3-rewrite-plan.md`. German domain names are kept 1:1 from v2 (`Motor`, `Einlass`, `Auslass`, `Auspuff`, `Ueberstroemer`, `Vergaser`, `Ausrollen`, `Geschwindigkeit`, field names like `hubL`, `bohrungD`) — do not translate them in code or the database; the UI is bilingual (EN/DE).

## Monorepo map

- `apps/api` (`@sim-tuning/api`) — NestJS 12 REST API (Express 5). Domain modules under `src/modules/` (`auth`, `email`, `email-template`, `bullmq`, `database`, `health`, `web-analytics`; domain modules `vehicle`, `dyno`, `environment` follow in later phases). Listens on port 3001.
- `apps/email-worker` (`@sim-tuning/email-worker`) — BullMQ worker that renders and sends transactional mails.
- `apps/web` (`@sim-tuning/web`) — Next.js 16 App Router PWA (`app/`): PWA shell and views (`components/sim/`), IndexedDB offline store (`lib/offline/offline-store.ts`), Service Worker (`public/sw.js`), Web App Manifest (`app/manifest.ts`), and components from `@sim-tuning/ui`.
- `apps/apple` (`@sim-tuning/apple`, planned) — Native Swift 6 iOS 18 app (`SimTuningIOS`) with shared `Packages/SimTuningKit`. No watchOS or widget targets.
- `packages/common` (`@sim-tuning/common`) — shared Zod schemas (single source of truth), DTOs, enums, and utilities; physics logic, unit conversion and audio DSP land here.
- `packages/database` (`@sim-tuning/database`) — Prisma 7 schema, SQL migrations, client, and seed script (PostgreSQL).
- `packages/logger` (`@sim-tuning/logger`) — Winston wrapper; log via `@sim-tuning/logger`, never `console.log`.
- `packages/ui` (`@sim-tuning/ui`) — shared shadcn-style component library.
- `packages/eslint-config`, `packages/typescript-config` — shared configs.

## Toolchain facts

- **yarn 1** workspaces + turbo. Never use npm or pnpm here.
- Node ≥ 22, TypeScript strict, pure ESM (`"type": "module"`).
- `@sim-tuning/api` builds via `nestgo`; `common`, `database`, `logger` build via `tsc` + `tsc-alias`.
- `@sim-tuning/common` and `@sim-tuning/database` are consumed from `dist/` — after changing them, rebuild before dependents consume them.

## Commands (repo root)

- `yarn run dev` / `build` / `lint` / `test` / `format` — turbo pipelines
- `yarn run db:generate` / `db:migrate -- <name>` / `db:deploy` / `db:push` / `db:seed` — Prisma tasks
- `yarn workspace @sim-tuning/<name> run <script>` — run a script in one workspace

## Local environment

- `docker compose -f compose.dev.yml up -d` starts the stack: `db`, `redis`, `mailpit`, `caddy`, `web`, `api`, `email-worker`, `db-migrate`.
- Host ports are offset so the stack can run next to luki-fitti: Postgres `5438`, Redis `6383`, Caddy `8083`, web `3005`, api `3006`, Mailpit `8026` (SMTP `1026`).
- Env files per app: `<app>/.env.local` (samples in `<app>/.env.sample`). Never commit `.env*.local`.
- Inside docker, services reach each other by **service name** (`db`, `redis`, `api`), not `localhost`.
- Next.js server-side fetch needs the **absolute** API URL — set `SERVER_API_URL=http://api:3001/api` (mirrors `API_URL`).

## Agent Instructions (APM)

- This repository manages agent instructions with [APM](https://microsoft.github.io/apm/) (`apm.yml`). Do not edit generated files directly — they are overwritten on every `apm install` / `apm compile`.
- Always-on rules live in `.claude/rules/` and `AGENTS.md` (repo root).
- All authoring happens in `.apm/` (instructions in `.apm/instructions/*.instructions.md`, skills in `.apm/skills/`). After any change run `apm install && apm compile` and commit `.apm/` sources and generated outputs together.
