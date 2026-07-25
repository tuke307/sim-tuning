# Phase 3 — NuGet Package Upgrade Log

> Branch `modernize/phase2-net11` (continuing). Status: **in progress** — package bumps applied, build being verified.
> Goal: upgrade packages to newest stable (preview where .NET 11 requires it); remove deprecated/unused deps; fix fallout.

## Strategy

The migration brief says "newest stable, mix preview where required." Authoritative latest versions came from `dotnet list package --outdated --include-prerelease`, cross-checked by 8 parallel research subagents (breaking changes, net11 compat, license). Key decision: **most non-essential packages have only preview/beta "latests" — keep current stable for those** (avoid prerelease churn) and **take preview only where net11 requires it** (Maui.Controls, EF Core) or where a package is incompatible with MAUI 11 at runtime (CommunityToolkit.Maui).

## Version decisions

### 🔴 Bumped — required for .NET 11 / runtime compat
| Package | From | To | Why |
|---|---|---|---|
| `Microsoft.Maui.Controls` | 10.0.10 | `11.0.0-preview.6.26360.8` | Aligns PackageReference with installed workload; required for net11 TFMs |
| `Microsoft.EntityFrameworkCore.*` (×3) | 10.0.0 | `11.0.0-preview.6.26359.118` | Native net11 target; **clears all NU1903 CVEs** (SQLite3MC 2.3.5 + SqlClient 7.0) |
| `CommunityToolkit.Maui` | 13.0.0 | `15.0.0` | 13/14 use internal MAUI APIs removed in MAUI 11 → runtime risk; 15.0.0 is the "Explicit .NET 11 Compatibility" release |

### 🟢 Bumped — stable, low risk
| Package | From | To |
|---|---|---|
| `CommunityToolkit.Maui.MediaElement` | 6.1.3 | `10.0.0` (event API unchanged; iOS/Mac retest deferred to device) |
| `CommunityToolkit.Mvvm` | 8.4.0 | `8.4.2` (patch) |
| `LiveChartsCore.SkiaSharpView.Maui` | 2.0.0-rc4.5 | `2.0.5` (**stable 2.x now exists**; Dyno APIs unchanged) |
| `FftSharp` | 2.1.0 | `2.2.0` (pure-managed, no API breaks) |
| `UnitsNet` | 5.75.0 | `5.75.1` (patch) |
| `Serilog` | 4.3.0 | `4.4.0` |
| `Serilog.Extensions.Logging` | 9.0.2 | `10.0.0` |
| `AsyncFixer` | 1.6.0 | `2.1.0` |
| `Microsoft.NET.Test.Sdk` | 18.0.1 | `18.8.1` |
| `coverlet.collector` | 6.0.4 | `10.0.1` |

### ⚪ Kept (no net11 build / only-prerelease / blocked / latest)
| Package | Version | Reason |
|---|---|---|
| `SkiaSharp` | 3.119.1 | LiveCharts 2.0.5 pins `SkiaSharp.Views.Maui.Controls ≥ 3.119.0`; bumping to 4.x alone → runtime `TypeLoadException`. Hold until LiveCharts bumps its pin. |
| `Sharpnado.Tabs.Maui` / `TaskLoaderView` | 4.0.1 / 2.6.3 | Already latest; **no net11/MAUI 11 build exists**, maintainer dormant ~9 mo. Built OK in Phase 2 (forward-compat); runtime binding risk on device. |
| `NAudio` | 2.2.1 | Windows-only (confirmed). **Don't bump.** Replacement plan: NLayer 2.0.1 (MP3 decode) + small custom WAV helper. Implementation deferred (~Phase 13). |
| `DBSCAN` | 3.0.0 | Already latest (repo: `viceroypenguin/DBSCAN`). |
| `MathNet.Numerics` | 5.0.0 | Latest stable; **used directly** (`Fit.PolynomialFunc` in `DynoAudioViewModel.cs:217`). |
| `Newtonsoft.Json` | 13.0.4 | Latest stable; migrate import path to `System.Text.Json` in Phase 14. |
| `Serilog.Sinks.File` / `Serilog.Sinks.Debug` | 7.0.0 / 3.0.0 | Already latest stable. |
| `StyleCop.Analyzers` | 1.1.118 | Last stable; 1.2.x is beta-only. |
| `Moq` / `xunit` | 4.20.72 / 2.9.3 | Already latest (v2 line). xunit v3 is a separate migration. |

### 🗑 Removed / relocated
- **`Microsoft.Maui.Controls.Compatibility`** — removed (unused: no `UseMauiCompatibility()`, no namespace refs; dropped in MAUI 11 Preview 6).
- **`CommunityToolkit.WinUI.Notifications`** — moved out of centralized `Directory.Build.props` → `SimTuning.Maui.App.csproj` as a **Windows-conditional** `<PackageReference Condition="'$(TargetFramework)'=='net11.0-windows10.0.19041.0'">` (consumed only in `Platforms/Windows/App.xaml.cs`; package is deprecated — migrate to `AppNotificationManager` later).
- **Data.csproj redundant `Update` overrides** (`Microsoft.Maui.Controls`, `UnitsNet`) — removed; inherit centralized versions.

### Other Phase 3 changes
- `Maui.App.csproj`: Android `SupportedOSPlatformVersion` 21.0 → **24.0** (MAUI 11 minimum).

## Applied changes (files)
- `src/Directory.Build.props` — all centralized version bumps + removals.
- `src/SimTuning.Data/SimTuning.Data.csproj` — EF Core ×3 → 11 preview; removed `Update` overrides.
- `src/SimTuning.Maui.App/SimTuning.Maui.App.csproj` — Android floor 24.0; Windows-conditional `WinUI.Notifications`.
- `src/Spectrogram/Spectrogram.csproj` — FftSharp → 2.2.0.
- `src/SimTuning.Test/SimTuning.Test.csproj` — NET.Test.Sdk → 18.8.1; coverlet → 10.0.1.
- `src/SimTuning.Maui.UI/Views/Popups/{Vehicle,DynoCreation,Environment,PortTiming}Popup.xaml` — **Popup v2 migration** (`Color`→`BackgroundColor`, `Size="W, H"`→`WidthRequest`/`HeightRequest`).
- `src/SimTuning.Maui.App/MauiProgram.cs` — `UseMauiCommunityToolkitMediaElement()` → `(isAndroidForegroundServiceEnabled: false)` (MediaElement 10.0.0 made the param required).

## Build fallout fixed (compiler-driven)
1. `MAUIX2002` ×24 on 4 popup XAMLs (`Color`/`Size` removed in Popup v2) → migrated to `BackgroundColor`/`WidthRequest`/`HeightRequest`.
2. `CS7036` on `MauiProgram.cs:28` (MediaElement 10.0.0 `UseMauiCommunityToolkitMediaElement` requires `isAndroidForegroundServiceEnabled`) → passed `false`.

## Verification — ✅ desktop green
- ✅ **NU1903 vulnerable-package warnings: 80 → 0** (EF Core 11 bump cleared SQLitePCLRaw + System.Security.Cryptography.Xml CVEs).
- ✅ **Full MacCatalyst app builds green** (`SimTuning.Maui.App.dll`, maccatalyst-arm64); all libraries + `SimTuning.Test` build on net11.
- ✅ Popup v2 migration + MediaElement API fix applied (compiler-driven) — **zero remaining code/package errors**.
- ❌ `Maui.App` `net11.0-ios` still blocked by the local iOS simulator-runtime mismatch (env, pre-existing — same as Phase 2); fixable with `xcodebuild -downloadPlatform iOS`.
- ⏭ `dotnet test` deferred (test *contents* are Phase 15; the test project itself builds).
- ⏭ Formatters (csharpier/prettier) not installed — setup deferred to Phase 4; Phase 3 changes are config + minimal style-matched XAML/cs.

## Carry-forward
- NAudio → NLayer cross-platform replacement (implementation, ~Phase 13).
- `WinUI.Notifications` → `AppNotificationManager` migration.
- SkiaSharp 3→4 when LiveCharts bumps its pin.
- Sharpnado net11 availability (watch; fork/replace if it breaks on device).
- MediaElement 10 + Mono→CoreCLR runtime validation on iOS/Mac (device testing).
- `dotnet test` runner adapter (`xunit.runner.visualstudio`) + test content fixes (Phase 15).

## Research sources (per-package)
8 parallel subagents researched: Maui.Controls, EF Core, LiveCharts/SkiaSharp, CommunityToolkit, Sharpnado, NAudio/FftSharp/DBSCAN, utilities, test packages. Key links captured in their reports; NuGet `--outdated` was the authoritative version source.
