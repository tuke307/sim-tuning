# Phase 9 — Logging posture (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: drop the production `Verbose` floor
> (Phase-1 A05 — PII/perf smell), make the Serilog level configuration-aware, and
> finish the structured-logging cleanup. Scope is **configuration + a few call-sites**;
> behavior of the app is unchanged (only *what gets logged where* changes).

## Result

| Sub-task | Outcome |
|---|---|
| Per-configuration Serilog level | `MinimumLevel.Verbose()` → `MinimumLevel.Is(minimumLevel)` with `#if DEBUG` `Debug` / `#else` `Warning`. Verbose floor gone. |
| Structured-logging anti-patterns | 3 message-as-template sites (`_logger.LogXxx(exc, exc.Message)`) → fixed templates. No `{0}` positional templates existed. |

**Build:** MacCatalyst head builds **green in both Debug and Release**, **0 errors, 0 new
warnings** (12, unchanged). The `#if DEBUG`/`#else` path is verified to compile in both
configurations.

## Applied (this phase)

### 1. `SetupSerilog` — per-configuration minimum level (the A05 fix)

`MauiProgram.SetupSerilog` set `MinimumLevel.Verbose()` unconditionally — in **Release**
that means every framework trace and every app event hits the rolling file at the
verbose level (the Phase-1 A05 smell: log volume + potential PII, since
`Information` events carry dyno/vehicle names).

- A grep confirmed **nothing in the app logs at Verbose/Trace** — `LogVerbose`/`LogTrace`
  are never called — so the Verbose floor captured nothing useful on the app side; it
  only lowered the bar for framework noise.
- Replaced with a configuration-selected floor:
  ```csharp
  #if DEBUG
      const LogEventLevel minimumLevel = LogEventLevel.Debug;   // full app-flow diagnostics
  #else
      const LogEventLevel minimumLevel = LogEventLevel.Warning; // production: problems only
  #endif
  ```
  via `.MinimumLevel.Is(minimumLevel)`. The `Microsoft → Warning` override, `Enrich.FromLogContext`,
  and both sinks (Debug + daily rolling File, 22 retained) are unchanged.
- **Effect:** Debug builds keep all diagnostics; Release logs only `Warning`/`Error`,
  minimizing production log volume and PII exposure. No app log output is *lost* (the
  dropped level was Verbose, which nothing used).

### 2. Structured logging — 3 message-as-template sites fixed

`_logger.LogError(exc, exc.Message)` is the structured-logging anti-pattern: the
exception message is used as the **message template**, so any `{...}` in it breaks
formatting and every unique message spawns a new template (defeating aggregation).
Fixed all three to fixed templates (matching the sibling pattern already used
elsewhere in `VehicleService`, e.g. `"Error retrieving environments"`):

| File | Was | Now |
|---|---|---|
| `VehicleService` (RetrieveDynos catch) | `LogError(exc, exc.Message)` | `LogError(exc, "Error retrieving dynos")` |
| `VehicleService` (RetrieveMotoren catch) | `LogError(exc, exc.Message)` | `LogError(exc, "Error retrieving motoren")` |
| `BrowserService` (OpenBrowser catch) | `LogError(ex, ex.Message)` | `LogError(ex, "Failed to open browser for {Url}", url)` |

> **No `{0}`/`{1}` positional templates** were found in any log call — the rest of the
> ~48 call sites already use named templates (`{DynoName}`, `{Message}`, …). So the
> Phase-1 "mixed structured/legacy templates" note reduced to these 3 sites.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| Runtime `LoggingLevelSwitch` (user-configurable level) | Over-engineering for this app; `#if DEBUG` covers the dev/prod split cleanly. | — (intentional) |
| PII redaction in log templates (e.g. scrub vehicle names) | `Warning` Release floor already excludes the `Information` events that carry names; deeper redaction is a dedicated concern. | follow-up (if needed) |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline).
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Release` — **0 errors, 12 warnings** (identical set; confirms the `#else` branch compiles and introduces nothing new).
  - iOS head environmentally blocked (Phase-2).
- **No residual `MinimumLevel.Verbose`** (grep clean).
- **No residual message-as-template log calls** (grep clean — all 3 fixed).
- **Warning baseline unchanged** (12): `Styles_Global` CS0618, `AuslassAnwendungView` MAUIG2045, `resources` CS8981, `DynoRuntime` CS0414/CS0169.
- No automated tests cover logging output; confidence rests on the build (both configs) + the static checks above.

## Carry-forward

- Optional: a `LoggingLevelSwitch` if user-configurable verbosity is ever wanted.
- Optional: PII redaction in templates if the Release log surface is ever broadened back to `Information`.
