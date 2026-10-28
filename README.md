# SimTuning

Two-stroke engine simulation and dyno tuning for **Web (PWA)** and **iPhone (iOS 18+)**.

> **v3 rewrite in progress.** This branch replaces the .NET MAUI app (kept on branch `v2`) with a
> TypeScript monorepo. The phased plan is in [`docs/v3-rewrite-plan.md`](docs/v3-rewrite-plan.md).
> Phase 0 (scaffold: auth, email, PWA shell, settings) is in place; the tuning modules follow.

---

## Modules

- **Vehicles:** vehicles with engine, intake, exhaust and transfer port data.
- **Engine (Motor):** displacement, compression, port timing diagram (Steuerdiagramm) and conversions.
- **Intake (Einlass):** intake resonance length and carburettor sizing.
- **Exhaust (Auslass):** expansion chamber theory and full exhaust layout.
- **Dyno:** power curves from recorded engine sound (spectrogram → RPM → DBSCAN clusters, computed
  on-device) and GPS speed / coast-down runs.
- **Bilingual (`EN` / `DE`):** German domain names are kept from v2; the UI is translated.

---

## Monorepo Structure

```text
sim-tuning/
├── apps/
│   ├── web/            # Next.js 16 + React 19 + Tailwind CSS v4 PWA
│   ├── api/            # NestJS 12 REST API (Express 5, Swagger, Cookie + Bearer Auth)
│   ├── email-worker/   # BullMQ transactional email worker (MJML templates)
│   └── apple/          # (planned) Native Swift 6 iOS 18 app + SimTuningKit
└── packages/
    ├── common/         # Shared Zod schemas, DTOs, enums, utils (physics & DSP to come)
    ├── database/       # Prisma 7 PostgreSQL schema, migrations & seed script
    ├── ui/             # Shared Radix UI / shadcn-style accessible components
    └── logger/         # Structured Winston logger
```

---

## Quick Start

### 1. Prerequisites

- **Node.js** `>= 22` and **Yarn** `1.22+`
- **PostgreSQL** and **Redis** (for API & email worker) — or use `compose.dev.yml`

### 2. Environment Setup

Copy the sample environment files to `.env.local`:

```bash
cp packages/database/.env.sample packages/database/.env.local
cp apps/api/.env.sample apps/api/.env.local
cp apps/email-worker/.env.sample apps/email-worker/.env.local
cp apps/web/.env.sample apps/web/.env.local
```

| Variable                  | Workspace                         | Description                                                                           |
| :------------------------ | :-------------------------------- | :------------------------------------------------------------------------------------ |
| `DATABASE_URL`            | `database`, `api`, `email-worker` | PostgreSQL connection URL                                                             |
| `REDIS_URL`               | `api`, `email-worker`             | Redis URL for BullMQ and access-token revocation                                      |
| `JWT_SECRET`              | `api`, `web`                      | Access token signing secret (`>= 32` chars in production)                             |
| `JWT_REFRESH_SECRET`      | `api`                             | Refresh token signing secret (`>= 32` chars in production)                            |
| `FRONTEND_URL`            | `api`                             | Allowed CORS origin (default `http://localhost:3000`)                                 |
| `API_PORT`                | `api`, `web`                      | Backend port (default `3001`)                                                         |
| `UMAMI_URL`               | `api`                             | Self-hosted Umami base URL                                                            |
| `UMAMI_WEBSITE_ID`        | `api`                             | Umami website ID; pageview forwarding is off when unset or outside production         |
| `UMAMI_ALLOWED_HOSTNAMES` | `api`                             | Comma-separated hostnames to forward pageviews for (default `simtuning.tony-luke.de`) |

### 3. Install, Migrate & Run

```bash
yarn install
yarn db:generate
yarn db:deploy
yarn db:seed

# Start development servers
yarn dev
```

Local docker stack (`docker compose -f compose.dev.yml up -d`) uses host ports offset from other
local projects: Postgres `5438`, Redis `6383`, Caddy `8083`, web `3005`, api `3006`, Mailpit `8026` (SMTP `1026`).

---

## Common Scripts

| Command                              | Description                                 |
| :----------------------------------- | :------------------------------------------ |
| `yarn dev`                           | Start development servers across workspaces |
| `yarn build`                         | Build all packages and applications         |
| `yarn test`                          | Run unit tests (`common`, `api`, `web`)     |
| `yarn lint`                          | Run TypeScript and ESLint checks            |
| `yarn db:migrate -- <name>`          | Create and apply a new Prisma migration     |
| `yarn --cwd apps/api openapi:export` | Export Swagger OpenAPI spec for the iOS app |

---

## Legacy v2 (.NET MAUI)

The previous .NET MAUI app lives on branch `v2` and stays there unchanged.
