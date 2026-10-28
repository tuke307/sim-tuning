---
applyTo: '**'
description: 'TypeScript conventions across the monorepo — strict typing, ESM, layering, and shared package usage'
---

# GitHub Copilot Instructions (TypeScript)

These instructions apply across the monorepo's TypeScript packages (Next.js web app, Express API, workers, shared libraries). They ensure Copilot outputs align with our strict TS configs, ESM modules, and layered architecture.

## 🧠 Context

- **Workspace**: Turbo-powered monorepo with shared packages under `packages/` and apps under `apps/`
- **Runtime targets**: Node ≥ 25 for backend, modern browsers for Next.js frontend
- **Module system**: Pure ESM (`"type": "module"`, TS `moduleResolution: bundler`); compiled output lives in `dist/`
- **Tooling**: `tsc`, `tsc-alias`, ESLint flat config, Prettier, Jest/Testing Library, Playwright, Turbo tasks

## 🔧 General Guidelines

- Stick to strict TypeScript. Favor explicit generics and discriminated unions instead of `any`.
- Use `type`/`interface` aliases to model DTOs, Prisma payloads, API responses, and component props.
- Prefer `async`/`await`; avoid promise chains that obscure control flow.
- Treat shared packages as the single source of truth—reuse helpers from `packages/common`, `packages/logger`, etc., rather than duplicating logic in apps.
- Keep source files referencing other `.ts/.tsx` files; only runtime entrypoints should import compiled JS.
- Keep imports sorted: builtin → external → `@sim-tuning/*` → local (`@/` alias or relative).
- Document non-trivial business logic with short comments or docblocks; keep comments purposeful.

## 📁 Monorepo Structure Reminders

```
apps/
  api/              // Express REST API
  web/              // Next.js site
  email-worker/     // Nodemailer-based worker
packages/
  common/           // Shared utils, polyfills, DTOs
  database/         // Prisma schema + client helpers
  logger/           // Winston wrapper
  eslint-config/    // Flat configs consumed via ESLint
  typescript-config // Base tsconfig presets
  ui/               // Shared UI components
```

When adding new modules, mirror the existing layout (e.g., `src/services`, `src/schemas`, `src/components`).

## 🧶 Patterns

### ✅ Do

- Enforce separation of concerns: controllers/controllers ↔ services ↔ repositories on the backend; Next.js components ↔ hooks ↔ data clients on the frontend.
- Guard inputs with Zod schemas or explicit TS types at the edges (request handlers, React server actions, CLI entrypoints).
- Use shared error utilities (`HttpError`, `handlePrismaError`, etc.) to keep error responses consistent.
- Prefer dependency injection via constructor params or factory functions in services to ease testing.
- Use `@sim-tuning/logger` for structured logging; no stray `console.log` in source.
- Keep configuration centralized (`src/config`, `packages/common/config`). Read from env once and pass values down.
- Write tests colocated with the code when practical (e.g., `__tests__`, `.test.ts`, or Playwright specs in `tests/`).

### 🚫 Avoid

- Importing from `dist/` inside `src/`.
- Casting to `any` to silence TS errors; resolve the underlying typing issue instead.
- Adding new `.env` accesses outside the sanctioned config helpers.
- Mixing CommonJS interop patterns (`require`, `module.exports`) with our ESM codebase.
- Writing complex logic directly inside React components or Express routes—extract helpers/services.
- Introducing duplicate schema definitions (e.g., redefining Zod schemas in multiple layers).

## 🧩 Example Prompts

- `Copilot, scaffold a Zod schema and DTO for ReservationSeason with optional notes.`
- `Copilot, add a Prisma repository method returning paginated seasonal hours with filtering.`
- `Copilot, create a React hook that wraps fetcher logic with SWR and our /api/reservations endpoint.`
- `Copilot, extend @sim-tuning/logger with a helper that logs sync metadata.`
- `Copilot, write a Jest test for bookingsService.cancel that asserts email notifications fire.`

## 🔁 Iteration & Review

- Always run `tsc --noEmit` + ESLint on touched packages; consider adding unit tests when behavior changes.
- When Copilot output is close but not perfect, add TODO comments or type hints and regenerate rather than fixing large chunks manually.
- Watch for path alias correctness after moving files—`tsc-alias` adjusts build output, but source imports must be accurate.
- Keep PRs scoped per domain (e.g., “seasonal settings service”) to simplify review.

## 📚 References

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [Turbo Repo Docs](https://turbo.build/repo/docs)
- [Next.js App Router](https://nextjs.org/docs/app)
- [Express 5](https://expressjs.com/)
- [Zod](https://zod.dev/)
- [Prisma](https://www.prisma.io/docs)
- [Jest](https://jestjs.io/docs/getting-started)
- [Playwright](https://playwright.dev/docs/intro)

## 📚 References

- [TypeScript Handbook](https://www.typescriptlang.org/docs/handbook/intro.html)
- [Zod Docs](https://zod.dev/)
- [Jest Docs](https://jestjs.io/docs/getting-started)
- [Project Style Guide](https://ts.dev/style/)
