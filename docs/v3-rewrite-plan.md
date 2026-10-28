# SimTuning v3 — Complete Rewrite Plan

Branch: v3 work is rebased onto `main` (which holds v2) and merged there; the .NET tree is removed
in the same change. The frozen .NET MAUI app stays on branch `v2`.
Reference stack: `luki-fitti` (same author) — identical tooling, folder layout and conventions.

## Decisions

| Topic      | Decision                                                                                    |
| ---------- | ------------------------------------------------------------------------------------------- |
| Scope      | Mirror luki-fitti: `web` + `api` + `email-worker` + `apple` (iOS only — no watchOS/widgets) |
| History    | Rebased onto `main`; branch `v2` stays as the frozen .NET MAUI legacy branch                |
| Naming     | Keep German domain terms 1:1 (`Auslass`, `Einlass`, `Ueberstroemer`, `Ausrollen`, `hubL`…)  |
| Audio dyno | Client-side only (Web Audio + TS DSP in browser, AVAudioEngine + vDSP on iOS)               |
| .NET       | Removed entirely — no `src/`, `.sln`, `csproj`, csharpier, stylecop, `global.json`          |

## 1. Inventory of v2 (what must be ported)

~25k LOC C# (7.2k of it is a generated icon font table). Projects:

| v2 project           | Content                                                                               | v3 target                                      |
| -------------------- | ------------------------------------------------------------------------------------- | ---------------------------------------------- |
| `SimTuning.Core`     | `ModuleLogic/{Engine,Einlass,Auslass,Dyno,Audio}Logic`, Quantity unit models, helpers | `packages/common/src/{logic,units,audio}`      |
| `Spectrogram`        | Vendored swharden/Spectrogram (MIT) + FftSharp, WAV reader                            | `packages/common/src/audio` (TS port, attrib.) |
| `SimTuning.Data`     | EF Core SQLite, 14 entities, seed (4 environments, 6 motors, vehicles), UnitSettings  | `packages/database` (Prisma 7 / Postgres)      |
| `SimTuning.Maui.UI`  | 25 ViewModels, 22 XAML views, 2 popups, theming (primary/accent palettes), navigation | `apps/web` + `packages/ui`                     |
| `SimTuning.Maui.App` | Platform heads (iOS, MacCatalyst, Android, Windows), bundled demo WAV                 | `apps/apple` (iOS) + `apps/web` PWA            |
| `SimTuning.Test`     | 47 xUnit tests (module logic, settings, VMs)                                          | `node:test` specs in `packages/common` + e2e   |
| resx (`de-DE/en-US`) | UI strings                                                                            | `apps/web/lib/i18n/locales/{de,en}.json`       |

Third-party replacements:

| v2 dependency     | Purpose                                 | v3 replacement                                                              |
| ----------------- | --------------------------------------- | --------------------------------------------------------------------------- |
| UnitsNet          | Unit conversion                         | Small in-house `units` module in `common` (8 quantity types, base units)    |
| LiveCharts / Skia | Charts, Steuerdiagramm, Auspuff drawing | `recharts` (charts) + hand-built SVG components (diagram/exhaust)           |
| FftSharp / NAudio | FFT, MP3→WAV, trimming                  | In-house radix-2 FFT + `AudioContext.decodeAudioData` (handles mp3/m4a/wav) |
| DBSCAN (NuGet)    | RPM-line clustering                     | In-house DBSCAN (~60 LOC) in `common/audio`                                 |
| EF Core / SQLite  | Persistence                             | Prisma 7 + Postgres (server) / IndexedDB offline store (web)                |
| MAUI Geolocation  | Speed + coast-down GPS runs             | `navigator.geolocation.watchPosition` (web), CoreLocation (iOS)             |
| ZIP export/import | Dyno sharing                            | Zod-validated JSON export/import via API (removes Zip-Slip surface)         |

Dropped: `DynoNmModel` (already `[Obsolete]`), Tizen/Android/Windows heads, `MaterialDesignIcons.cs`
(→ `lucide-react`), 18×2 theme palette XAML files (→ design tokens in `packages/ui/globals.css`),
the WordPress demo-download URL (→ bundled demo dataset in `common`).

## 2. Target structure (mirrors luki-fitti)

```text
sim-tuning/
├── apps/
│   ├── web/            # Next.js 16 + React 19 + Tailwind v4 PWA (next-intl EN/DE, offline IndexedDB)
│   ├── api/            # NestJS 12 REST API (Express 5, Swagger, cookie + bearer auth, throttler, helmet)
│   ├── email-worker/   # BullMQ transactional mail worker (MJML templates)
│   └── apple/          # Swift 6, iOS 18 app + SimTuningKit Swift package, XcodeGen project.yml
├── packages/
│   ├── common/         # Zod schemas/DTOs, enums, units, physics logic, audio DSP, seed catalog
│   ├── database/       # Prisma 7 schema, migrations, seed
│   ├── ui/             # Radix/shadcn components (copied from luki-fitti, trimmed)
│   ├── logger/         # Winston logger (verbatim)
│   ├── eslint-config/  # verbatim
│   └── typescript-config/ # verbatim
├── bruno/              # API collection
├── docs/
├── .github/{workflows/production.yaml,instructions/}
├── .husky/ .apm/ .claude/ .agents/ .vscode/
├── compose.yml compose.dev.yml Caddyfile.dev turbo.json package.json yarn.lock
└── AGENTS.md PRODUCT.md README.md .commitlintrc.json .lintstagedrc .prettierrc.json …
```

Package scope: `@luki-fitti/*` → `@sim-tuning/*`. Tooling: Yarn 1.22 workspaces, Turbo, TS 6.0.3,
Prettier, ESLint 10, husky + commitlint + lint-staged — copied 1:1.

Do **not** copy from luki-fitti: `.env*`, `.gitconfig.local`, `graphify-out/`, `gymlife-extracted/`,
`mci-app/`, `node_modules/`, `.turbo/`, `.build/`, `playwright-report/`, `test-results/`, gym code.

## 3. Domain model (Prisma)

German names kept; Prisma models PascalCase, fields camelCase, values stored in **base units**
(same as v2 `*BaseUnit` statics: mm, mm², mm³, kg, m/s, °C, hPa, ms).

- Auth (from luki-fitti verbatim): `User`, refresh/verification/reset tokens, preferences
  (language, theme, `roundingAccuracy`, `roundOnUnitChange`, per-field unit choices).
- `Vehicle` (name, beschreibung, gewicht, `systemKey?`, `userId?`) 1—1 `Motor`, 1—1 `Dyno`
- `Motor` (hubL, bohrungD, pleulL, deachsierungL, hubraumV, brennraumV, kurbelgehaeuseV,
  verdichtungV, resonanzU, kolbenG, heizwertU, zuendzeitpunkt, zylinderAnz)
  1—1 `Einlass` (1—1 `Vergaser`), 1—1 `Auslass` (1—1 `Auspuff`), 1—1 `Ueberstroemer`
- `Dyno` (name, beschreibung, active, environmentId) 1—n `DynoPs`, `Drehzahl`, `Ausrollen`, `Geschwindigkeit`
- `Environment` (name, temperaturT, luftdruckP, …)

Seed rows (4 environments, 6 motors, demo vehicles) become `systemKey` starter content
(pattern from luki-fitti migration `starter_content_system_key`), replacing `Deletable=false`.
Every user-owned row carries `userId`; API enforces ownership (deny by default).

## 4. Package-by-package work

### packages/common (first — everything depends on it)

- `units/`: length, area, volume, mass, speed, temperature, pressure, duration converters + rounding.
- `logic/engine.ts`, `einlass.ts`, `auslass.ts`, `dyno.ts`: pure functions, German names kept
  (`getCompression`, `getResonanzLaenge`, `getVergaserDurchmesser`, `getLeistung`, …).
- `logic/steuerdiagramm.ts`, `logic/auspuff.ts`: return **geometry** (arcs, polyline points) instead
  of `SKBitmap`; web/iOS render it as SVG / SwiftUI `Path`.
- `audio/`: `fft.ts`, `spectrogram.ts` (Hann window, fftSize 16384, minFreq 25, maxFreq 250,
  stepSize = len/1000), `hotPoints.ts` (intensity threshold), `dbscan.ts` (ε = 8, minPts = 5),
  `toRotationalSpeed` (Hz·60 → U/min).
- `schemas/`: Zod for every entity + export file format (versioned `{ version: 3, dyno: … }`).
- Tests: port all `SimTuning.Test/ModuleLogic/*` as golden-value `node:test` specs
  (run the v2 xUnit suite once to capture expected numbers before deleting nothing — v2 stays on its branch).

### packages/database

Schema + initial migration `…_init_sim_tuning`, seed script, `Dockerfile.migration`. Copy config from luki-fitti.

### apps/api (NestJS)

Keep: `auth`, `bullmq`, `database`, `email`, `email-template`, `health`, `web-analytics`, guards,
filters, interceptors, pipes, config. Drop gym modules (`exercise`, `session`, `stretching`, `workout`, `analytics`).
New modules: `vehicle` (incl. nested motor/einlass/auslass/ueberstroemer upserts), `dyno`
(+ points bulk replace, export/import JSON), `environment`. Swagger + `openapi:export` to `apps/apple`.

### apps/email-worker

Verbatim copy, rebrand templates (verify-email, reset-password, account-deletion).

### apps/web (Next.js PWA)

Same shell pattern as luki-fitti (`app/page.tsx` + `components/<domain>/pwa-shell.tsx`, `(auth)` routes,
providers for auth/i18n/theme, `lib/offline/offline-store.ts`). Views under `components/sim/`:

| v2 view(s)                                                               | v3 view                             |
| ------------------------------------------------------------------------ | ----------------------------------- |
| Home, MainPage                                                           | `home-view.tsx`, `pwa-shell.tsx`    |
| Vehicles + VehiclePopup                                                  | `vehicles-view.tsx`, vehicle dialog |
| Motor: Hubraum, Verdichtung, Steuerdiagramm, Umrechnung, PortTimingPopup | `motor/*-view.tsx`                  |
| Einlass: Kanal, Vergaser                                                 | `einlass/*-view.tsx`                |
| Auslass: Theorie, Anwendung                                              | `auslass/*-view.tsx` (SVG exhaust)  |
| Dyno: Data, Audio, Diagnosis + Runtime/Geschwindigkeit/Ausrollen modals  | `dyno/*` (recharts)                 |
| Einstellungen, Environment                                               | `settings-view.tsx`                 |

Audio dyno flow: file upload or `MediaRecorder` → `decodeAudioData` → mono Float32 →
Web Worker runs `common/audio` → recharts scatter of RPM points + clusters → user picks cluster
→ saved as `Drehzahl` points. Audio never leaves the device.
GPS runs: `watchPosition` (HTTPS, foreground only; show warning) → `Geschwindigkeit`/`Ausrollen`.
Tests: `lib/**/*.spec.ts` + Playwright e2e (auth, vehicle CRUD, dyno audio with bundled demo WAV).

### apps/apple (iOS 18 only)

XcodeGen `project.yml` with a single `SimTuningIOS` target + `SimTuningIOSUITests`;
`Packages/SimTuningKit` (API client from OpenAPI, offline store, Swift port of physics + DSP via
Accelerate/vDSP). AVAudioEngine recording, CoreLocation for GPS runs (better than browser: background-capable).
No watch target, no widgets. Mac support via "Designed for iPad" (replaces MacCatalyst head).

## 5. Infra & CI

- `compose.yml` / `compose.dev.yml`: db, db-migrate, redis, mailpit, api, web, email-worker, caddy.
  Ports must not collide with luki-fitti (5436/8082/3002/3003/6382) or langfuse (5437/3004/6379):
  dev stack uses `5450`, `8090`, `3010` (web), `3011` (api), `6390`, Mailpit `1030`/`8030`, its own
  compose project `sim-tuning-dev`, database `simtuning_dev`, user `simtuning` and volumes.
- Images: `ghcr.io/tuke307/sim-tuning-{web,api,email-worker,db-migrate}`.
- `.github/workflows/production.yaml` copied; trigger on push to `v3` until cutover, then the default branch.
- Domain: `simtuning.tony-luke.de` (already used by v2 for demo data).

## 6. Execution phases (each = one PR into `v3`)

| #   | PR                                              | Done when                                                       |
| --- | ----------------------------------------------- | --------------------------------------------------------------- |
| 0   | `chore: scaffold monorepo from luki-fitti`      | `yarn install && yarn build && yarn lint` green with empty apps |
| 1   | `feat(common): port units + physics logic`      | golden tests match v2 xUnit values                              |
| 2   | `feat(common): port spectrogram + dbscan`       | demo WAV yields same RPM points/clusters as v2 (± rounding)     |
| 3   | `feat(db): prisma schema, migration, seed`      | `yarn db:deploy && yarn db:seed` on fresh Postgres              |
| 4   | `feat(api): auth + vehicle/dyno/environment`    | bruno collection passes; ownership checks covered by tests      |
| 5   | `feat(email-worker): rebranded worker`          | mailpit receives verify/reset mails                             |
| 6   | `feat(web): shell, auth, i18n, settings`        | login → empty home, EN/DE switch, theme                         |
| 7   | `feat(web): vehicles + motor modules`           | parity checklist rows for Motor ticked                          |
| 8   | `feat(web): einlass + auslass modules`          | SVG exhaust matches v2 drawing for seed vehicle                 |
| 9   | `feat(web): dyno (data, audio, gps, diagnosis)` | e2e audio-dyno test green                                       |
| 10  | `feat(web): offline store + pwa`                | works in airplane mode, syncs on reconnect                      |
| 11  | `feat(apple): ios app + SimTuningKit`           | `swift test` + UI tests green                                   |
| 12  | `ci: production workflow + compose`             | images build and deploy                                         |
| 13  | Cutover                                         | see below                                                       |

Phases 7–11 can run in parallel after 6 (apple only needs 1–4).

## 7. Cutover

1. Parity checklist (one row per v2 view) signed off.
2. Tag the `v2` head as `v2-final`; mark the branch read-only / archived in README.
3. Default branch is already `main`; merging v3 into `main` replaces the .NET app there.
4. Retarget open PRs; update store listings / links to the PWA.

## 8. Risks & open points

- **Numeric parity**: v2 logic has quirks (e.g. `GetSteigungskraft` treats `s == 0` as `1`,
  `GetLeistung` returns W not PS). Port as-is behind golden tests, fix in a separate PR.
- **Browser audio**: iOS Safari records AAC/mp4 — fine via `decodeAudioData`; 16384-pt FFT on long
  files must run in a Worker to keep UI responsive.
- **Browser GPS**: foreground only, lower rate than native; the iOS app is the recommended dyno-run device.
- **License**: repo is public; luki-fitti is `UNLICENSED`. Decide v3 license before copying config
  (vendored Spectrogram port must keep its MIT notice).
- **Accounts**: v2 had no login. Decide whether guest/offline-only use is allowed without an account.
- **Data migration**: v2 data lives in on-device SQLite; offer JSON import of v2 `DataExport.json` (optional).
