# Phase 8 — DI / DbContext lifetime (partially complete; 2 sub-tasks deferred)

> Branch `modernize/phase2-net11`. Roadmap scope: (1) `DatabaseContext`
> Singleton→Scoped, (2) remove the 23 `Ioc.Default` View call-sites,
> (3) decouple `SimTuning.Data` from `Microsoft.Maui.Storage`.
>
> **Sub-task 1 is done (behavior-preserving, build-verified).** Sub-tasks 2 & 3
> turned out — on investigation — to be large architectural changes with no
> runtime/device verification path and high behavior-regression risk, so they are
> **deferred with precise technical rationale** below (same discipline as Phase 5/6/7
> deferrals). This is reported honestly rather than forced.

## Result

| Sub-task | Outcome |
|---|---|
| `DatabaseContext` lifetime | **Done.** `AddSingleton<DatabaseContext>()` → `AddDbContextFactory<DatabaseContext>()`; `VehicleService` injects `IDbContextFactory<DatabaseContext>`; the 15 `new DatabaseContext()` calls → `_dbFactory.CreateDbContext()`. |
| 23 `Ioc.Default` View sites | **Deferred.** Blocked by the Shell `ContentTemplate` DataTemplate (parameterless-ctor) constraint. |
| Data ↔ `Microsoft.Maui.Storage` decoupling | **Deferred.** Wide static→instance ripple (touches `OnConfiguring` + every `DatabaseSettings`/`GeneralSettings` consumer); Phase-1 P3. |

**Build:** MacCatalyst head builds **green, 0 errors, 0 new warnings** (still 12 —
unchanged from Phase 7; no EF warnings from the factory registration).

## Applied (this phase)

### Sub-task 1 — `DatabaseContext` via `IDbContextFactory` (the Phase-1 "standout bug")

**What was actually wrong.** `MauiProgram` registered `DatabaseContext` as
`AddSingleton` — a captive, app-lifetime DbContext (the EF foot-gun). But
investigation showed **no consumer resolved it from DI**: `VehicleService` (the sole
DB user) `new`'d its own `DatabaseContext()` per method (15 sites incl. the ctor).
So the Singleton registration was *dead*, and the real smell was the service-locator
`new DatabaseContext()` spread across `VehicleService`.

**Fix (the EF-Core-correct pattern, behavior-preserving):**

- `MauiProgram`: `services.AddSingleton<DatabaseContext>()` → `services.AddDbContextFactory<DatabaseContext>()`. This registers `IDbContextFactory<DatabaseContext>` (Singleton) and `DatabaseContext` (Scoped, created on demand). `OnConfiguring` still supplies the SQLite connection string from `DatabaseSettings.DatabasePath` (unchanged).
- `VehicleService`: injects `IDbContextFactory<DatabaseContext>`; the 15 `new DatabaseContext()` → `_dbFactory.CreateDbContext()`. Each is still a short-lived `using var` context — **identical per-operation lifetime to before.**
- `VehicleService` stays **Singleton**. Its in-memory caches (`Dynos`/`Vehicles`/`Environments`/`Motoren` — read by `RetrieveDynos()` etc.) are Singleton-dependent; preserving the lifetime keeps that caching behavior byte-identical. Because the factory (not a Scoped context) is the Singleton-captured dependency, there is **no captive-dependency** problem — this is exactly what `AddDbContextFactory` is designed for.

**Why this beats a naive Singleton→Scoped flip:** making `DatabaseContext` Scoped while
`VehicleService` stays Singleton would create a captive dependency (the Singleton
captures the Scoped context → effectively Singleton again, worse). And making
`VehicleService` Scoped/Transient would change its cache semantics. The factory keeps
both lifetimes correct.

`IVehicleService` is unchanged (no `DatabaseContext` on its surface), so the
`Mock<IVehicleService>` test is unaffected; no test constructs `VehicleService`
directly.

## Deferred (with rationale)

### Sub-task 2 — 23 `Ioc.Default` View call-sites

Every site is `BindingContext = Ioc.Default.GetRequiredService<XxxViewModel>()` inside
a View's **parameterless** constructor. Investigation found `MainPage` is a **`Shell`**
whose items are declared as:

```xml
<ShellContent ContentTemplate="{DataTemplate home:HomeView}" />
```

Shell's `ContentTemplate` DataTemplate **instantiates pages via their parameterless
constructor** — it cannot perform DI constructor injection. Therefore these Views
cannot take constructor-injected VMs without abandoning `ContentTemplate` for
`Routing.RegisterRoute` + `Shell.Current.GoToAsync(...)` (a navigation-model overhaul
across the flyout structure), and **none of it is runtime-verifiable here** (no
device/simulator; `dotnet test` is 0-discovered — Phase 15; MacCatalyst build is the
only green target).

Doing this blind would risk breaking the app's entire navigation. The service-locator
call is, pragmatically, the established MAUI Shell workaround for DI'd VMs on
DataTemplate-created pages.

**To unblock:** restructure Shell navigation to registered routes (each View
`AddTransient`-registered in DI, ctor-injected VM), then verify on device. → dedicated
follow-up.

### Sub-task 3 — `SimTuning.Data` ↔ `Microsoft.Maui.Storage` decoupling

`DatabaseSettings` is a **static** class calling `Preferences.Default.Get/Set` and
`FileSystem.AppDataDirectory` directly, and is consumed as `DatabaseSettings.FileDirectory`
/ `.DatabasePath` (and via `GeneralSettings.*FilePath`) across Data, Core, and UI. It is
also read inside `DatabaseContext.OnConfiguring` (EF context-creation time). Decoupling
requires introducing a settings abstraction implemented by the MAUI app and converting
the static class to instance/dependency-injected — a wide ripple across every consumer
plus non-trivial injection into `OnConfiguring`. Phase-1 rates this **P3** (lowest
priority). Not blind-safe.

**To unblock:** a focused `IPlatformSettings` abstraction + instance `DatabaseSettings`
injected through `DbContextOptions` (replacing the `OnConfiguring` static read). →
dedicated follow-up (or fold into a Settings-layer refactor).

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst` — **0 errors, 0 new warnings** (12, unchanged from Phase 7). No EF warnings from `AddDbContextFactory`.
  - `SimTuning.Test` — **0 errors** (pre-existing test warnings, Phase 15). Confirms the `VehicleService` ctor change didn't break test compilation (tests mock `IVehicleService`).
  - iOS head environmentally blocked (Phase-2).
- **No `new DatabaseContext()` residuals** in `VehicleService` (all 15 → `_dbFactory.CreateDbContext()`).
- **`IVehicleService` surface unchanged** (no `DatabaseContext`) → mock-based tests unaffected.
- **No automated service tests** (`dotnet test` 0-discovered — Phase 15). Confidence rests on the behavior-preserving nature of the factory swap (per-op short-lived context, identical lifetime; Singleton service + cache preserved). A device smoke test (create/save/retrieve/delete a dyno) is recommended to confirm the DB path end-to-end.

## Carry-forward

- **Sub-task 2:** Shell→registered-routes navigation restructure + device verification (removes the 23 `Ioc.Default` View sites).
- **Sub-task 3:** `IPlatformSettings` abstraction + instance `DatabaseSettings` (decouples Data from MAUI.Storage; replaces the `OnConfiguring` static path read).
- Device smoke test of the full CRUD flow (exercises `IDbContextFactory` + `EnsureDatabaseCreated`).
- `EnsureDatabaseCreated()` still calls both `Migrate()` and `EnsureCreated()` (Phase-1 §7.4 anti-pattern) → Phase 12.
