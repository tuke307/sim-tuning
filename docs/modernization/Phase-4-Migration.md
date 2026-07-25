# Phase 4 — Fix Compilation (✅ Complete)

> Branch `modernize/phase2-net11` (off `v2.0.0`). SDK `11.0.100-preview.6`.
> Goal: resolve compiler/analyzer warnings **properly** — no new `<NoWarn>`, no blanket `!`/`#pragma`.

## Result

**Unique solution warnings: 484 → 24.** **0 non-iOS errors** (the 3 errors are the pre-existing iOS simulator-runtime env block; MacCatalyst builds green). Production code is **warning-clean** — all 24 remaining are mapped to later phases (§Deferred).

Commits: `72d8447` (obsolete APIs + formatter setup), `3e42c01` (XAML deprecations), `03a5626` (nullable migration Core+VMs), `323c42a` (nullable tail + analyzer stragglers). Plus `8fd8acd` (this doc).

`dotnet test`: still **0 tests discovered** — `xunit.runner.visualstudio` adapter is missing from `SimTuning.Test.csproj` (pre-existing; Phase 15). The test project itself builds.

## What was fixed

### Obsolete APIs (commit `72d8447`)
- **SkiaSharp 3.x SKFont migration** (CS0618 ×22): `SKPaint.TextSize` + `DrawText(string,x,y,SKPaint)` obsolete → dropped `TextSize` from paints, added one `SKFont labelFont` per diagram, switched to `DrawText(s,x,y,SKTextAlign.Left,labelFont,blackPen)`. `AuslassLogic` (×19) + `EngineLogic` (×3). Rendering unchanged.
- **`Functions.cs`**: removed dead private `Generate128BitsOfRandomEntropy()` (sole `RNGCryptoServiceProvider` → SYSLIB0023) + orphaned using.
- **`NavigationService.cs`**: `Application.MainPage` (deprecated MAUI 11) → `Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation`.

### XAML deprecations (commit `3e42c01`)
- **`*AndExpand` layout options** (CS0618 ×173): → base options across 17 XAML (168 tokens). Standard MAUI migration; ⚠️ may shift layouts with extra space — **flag for device visual review**.
- **`FontSize="<NamedSize>"`** (CS0612 ×21): shorthand routed through deprecated `Device.GetNamedSize` (platform-**dynamic** values). → explicit doubles on the app's `Global.xaml` scale (Medium=14, Subtitle=16, Caption=12).

### Nullable-reference migration (commits `03a5626`, `323c42a`)
- **Core**: `Converts`, `AudioUtlis`, `AudioLogic`, `DynoLogic` (`(double)X`→`X ?? 0`), `AuslassLogic` (`.Value`→`.GetValueOrDefault()`, 9 sites), `VehicleService`+`IVehicleService` (8 retrieve/create returns → nullable), `Functions.UpdateValue` (params → `UnitListItem?`, cleared ~24 caller CS8624 at source).
- **Behaviors/Converters**: `BehaviorBase.AssociatedObject` → `T?`; `EventToCommandBehavior` (`Delegate?`, `object?`, guards); value converters `IValueConverter` params → nullable + pattern-match bodies.
- **ViewModels** (parallel subagents): Motor+Einlass (6), Auslass (2), Vehicles god-class (2), Dyno+small (9). Uniform pattern: nullable backing fields/props, `.GetValueOrDefault()` for `double?` unboxing, `object? sender` for handlers. ~10 justified `!` at null-tolerant boundaries (popup results, try/catch-wrapped saves) — each with an inline comment.

### Misc
- Formatter setup: `.csharpierrc.json` `useTabs:true→false` (config bug — codebase is space-indented; tabs caused SA1027 floods); `.csharpierignore` for generated/vendored C#.
- `Spectrogram.csproj` `<Nullable>disable</Nullable>` (vendored, Phase 1 recommendation).
- `DatabaseContext` SA1128 (ctor initializer on own line); dead field `_helperVehicle` removed (CS0169).

## ⛔ Deferred (the remaining 24 — all mapped)

| Item | Count | Phase | Note |
|---|---|---|---|
| `ListView` deprecated → CollectionView | 6 | 6/12 | `Styles/Global.xaml` style; UI-control migration, behavior risk |
| `MAUIG2045` binding source-gen hint | 3 | 6 | works via reflection; property likely `[ObservableProperty]`-generated |
| `AsyncFixer02` sync IO in async | 2 | 7 | `DynoDataViewModel`: `File.WriteAllText`, `ZipFile.ExtractToDirectory` |
| Test `CS8625`/`CS8602`/`xUnit1008` | 11 | 15 | null args to VM methods, dead assertions, missing `[Theory]` |
| Dead fields `CS0169`/`CS0414` | 2 | 16 | `DynoRuntime._locationService` (readonly DI), `trackingStarted` |

## 🚨 Gotchas (carry forward)

1. **`.csharpierrc.json` `useTabs` must stay `false`.** Codebase is space-indented; StyleCop SA1027 enforces spaces. `.editorconfig` says `indent_style=tab` but real code + SA1027 want spaces (separate debt).
2. **NEVER run `csharpier format <directory>` (recursive).** It triggers a phantom prettier pass that reformats ~70 XAML/XML/csproj/props files to tabs. Use **single-file** `csharpier format <file>` only.
3. **`dotnet` at `/usr/local/share/dotnet/dotnet`, not on PATH.** `export PATH="/usr/local/share/dotnet:/Users/MEISSTO/.dotnet/tools:$PATH"`.
4. **iOS build error is environmental** (missing simulator runtime: `xcodebuild -downloadPlatform iOS`). MacCatalyst is the green desktop target.
5. `global.json` pins `11.0.100-preview.6` (`allowPrerelease:true`); no .NET 10 side-by-side.
6. cspell installed to node_modules but **not** a committed devDep (German UI = noisy; deferred).
7. VM nullable edits were subagent-produced without per-file builds; the central build was the first verification (mop-up in commit `323c42a`).

## Decisions & rationale
- **Parallelize nullable across subagents**: 224 hand-written CS86xx across ~30 VMs, uniform patterns; disjoint file ownership, orchestrator builds centrally. (One subagent batch hit a 429 mid-run; its residual CS8622 derefs were finished by the orchestrator.)
- **`_engines`/`_vehicles` use `= new ObservableCollection<T>()` not `?`** (Vehicles god-class): set in ctor via `ReloadData()`; `= new()` is honest and avoids ~8 cascade sites. (Explicit type, not target-typed `new()` — SA1000 flags `new()`.)
- **VehicleService returns nullable** (not `return new List<T>()`): preserves behavior (null-on-error stays null, just honestly typed); caller mop-up is the cost.
- **`AndExpand`→base not Grid-restructure**: lightweight documented MAUI migration; Grid is invasive across 17 XAML. Visual delta flagged.
- **~10 justified `!`**: only at null-tolerant boundaries where the alternative is a behavior change (popup results that legitimately may be null; try/catch-wrapped dyno saves where the NRE is the error path). Each has an inline comment. Not blanket.
