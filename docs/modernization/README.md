# SimTuning Modernization — Living Docs

Continuous documentation for the .NET 10 → .NET 11 modernization (branch `v2.0.0`).
Per the roadmap, docs are **seeded in Phase 1 and refreshed every phase** — not written at the end.

## Index

| Doc | Purpose |
|---|---|
| [Phase-1-Analysis.md](./Phase-1-Analysis.md) | Full project analysis: structure, per-project findings, security, .NET 11 risk register, TODO → phases |
| [Phase-2-Migration.md](./Phase-2-Migration.md) | .NET 11 TFM migration log: changes, decisions, verification, environment setup |
| [Phase-3-Migration.md](./Phase-3-Migration.md) | NuGet package upgrade log: version decisions, Popup v2 migration, NU1903 CVE clearance |
| [Phase-4-Migration.md](./Phase-4-Migration.md) | Fix-compilation log: obsolete APIs, XAML deprecations, nullable-reference migration (**in progress** — resumable from doc) |

## Phase 2 — ✅ Complete (migrate TFMs to .NET 11)

**Outcome:** all 6 projects build on .NET 11 (`11.0.100-preview.6`). The full **MacCatalyst desktop app builds green**, and every library compiles on `net11.0`, `net11.0-ios`, and `net11.0-maccatalyst`. The only remaining build failure is `SimTuning.Maui.App` on `net11.0-ios`, blocked by a **local iOS simulator-runtime mismatch** (environmental, pre-existing — the README already documents iOS simulator issues), fixable with `xcodebuild -downloadPlatform iOS`.

**Beyond the brief's scope (justified):** the migration exposed that `SimTuning.Test` inherited mobile TFMs (so it was being built as an iOS/Mac *app bundle*). The proper fix — pin Test to `net11.0`-only and give `Core`/`Maui.UI`/`Spectrogram` a `net11.0` base target — was done here rather than deferred, because it's required for the solution to build and is the conventional MAUI test setup. Test *contents* (assertions, cross-platform paths, missing `xunit.runner.visualstudio` adapter) remain Phase 15.

See [Phase-2-Migration.md](./Phase-2-Migration.md) for the full change list and decisions.

## Phase 3 — ✅ Complete (upgrade NuGet packages)

**Outcome:** all packages upgraded to newest stable (preview only where .NET 11 requires). **NU1903 vulnerable-package warnings cleared (80 → 0)** via the EF Core 11 bump. The full MacCatalyst app builds green with all upgraded packages; zero code/package errors remain (only the pre-existing iOS simulator-runtime env block).

**Highlights:** Maui.Controls→`11.0.0-preview.6`, EF Core→`11.0.0-preview.6`, CommunityToolkit.Maui→`15.0.0` (the .NET 11 compat release — drove a **Popup v2 migration** on 4 popups), MediaElement→`10.0.0` (new required `isAndroidForegroundServiceEnabled` arg), LiveCharts `rc4.5`→`2.0.5` (stable!), FftSharp→`2.2.0`. Removed unused `Maui.Controls.Compatibility`; made `WinUI.Notifications` Windows-conditional. Kept SkiaSharp 3.119.1 (LiveCharts pins it), Sharpnado (no net11 build), NAudio 2.2.1 (Windows-only — replacement plan documented).

See [Phase-3-Migration.md](./Phase-3-Migration.md) for the full version table and decisions.

## Phase 4 — 🚧 In progress (fix compilation/analyzer warnings)

**Goal:** resolve compiler/analyzer warnings properly — no new `<NoWarn>`, no blanket `!`/`#pragma`. **Baseline 484 unique warnings → in progress.** Resumable from [Phase-4-Migration.md](./Phase-4-Migration.md).

**Done so far:**
- ✅ Obsolete APIs (committed `72d8447`): SkiaSharp `SKFont` migration (AuslassLogic×19, EngineLogic×3), removed dead `RNGCryptoServiceProvider` (SYSLIB0023), `Application.MainPage`→`Windows` (NavigationService). Plus formatter setup — **`.csharpierrc.json useTabs true→false`** (the codebase is space-indented; `useTabs:true` caused a 362-warning SA1027 flood).
- ✅ XAML deprecations (committed `3e42c01`): `*AndExpand`→base (168 tokens, 17 files); `FontSize` NamedSize shorthand→explicit doubles. CS0612 21→0.
- ✅ Core nullable (uncommitted, **Core builds clean: 0 CS86xx**): Converts, AudioUtlis, AudioLogic, DynoLogic, AuslassLogic, VehicleService+IVehicleService (8 returns→nullable), Behaviors (BehaviorBase, EventToCommandBehavior), Styles SA1027.
- ✅ VM nullable (subagents, uncommitted, **not yet build-verified**): Motor+Einlass, Auslass, Vehicles god-class done; Dyno batch still running.

**Remaining:** central build + cascade mop-up (VehicleService nullable returns cascade to callers; subagent-flagged `.UnitEnumValue`/`?.`-chain CS8602 sites); dead-field removal (`_helperVehicle`, `_locationService`, `trackingStarted`); verify/format/docs/commit. **Deferred:** AsyncFixer02→P7, xUnit1008 + test CS8625→P15, ListView CS0618→P6/12, MAUIG2045→P6.

⚠️ **Gotchas (see Phase-4 doc):** keep `.csharpierrc useTabs:false`; NEVER recursive `csharpier format` (phantom prettier); `dotnet` not on PATH; iOS build error is environmental.

## Environment setup (performed in Phase 2)

- SDK: only `11.0.100-preview.6.26359.118` installed (no .NET 10 side-by-side).
- **Workloads installed**: `sudo dotnet workload restore src/SimTuning.sln` → `maui-ios`, `maui-maccatalyst`.
- **Xcode selected**: `sudo xcode-select -s /Applications/Xcode.app/Contents/Developer`.
- **Xcode license accepted**: `sudo xcodebuild -license accept`.
- `global.json` pins the preview SDK (`allowPrerelease: true`).
- ⏳ **iOS simulator runtime**: mismatched with Xcode — run `xcodebuild -downloadPlatform iOS` to enable `net11.0-ios` app builds (optional for migration verification; MacCatalyst already proves the net11 app build).
- ⏳ **Formatters**: `csharpier` (global tool) and `prettier` (`npm install`) are **not yet installed** — set up at the start of Phase 4.

## Running TODO (append after every phase)

- [x] **Phase 2** — TFM migration done. Carried forward: iOS simulator runtime download; formatter install (Phase 4); Test-TFM restructure is done, test *contents* still Phase 15.
- [x] **Phase 3** — package upgrades done; NU1903 CVEs cleared (80→0); Popup v2 migrated. Carried forward: NAudio→NLayer replacement (~Phase 13); SkiaSharp 3→4 when LiveCharts bumps; Sharpnado net11 watch; MediaElement/Mono→CoreCLR device retest; `WinUI.Notifications`→`AppNotificationManager`.
- [~] **Phase 4** — 🚧 in progress. Baseline 484 unique warnings. Done: obsolete APIs (SkiaSharp `SKFont`, RNGCryptoServiceProvider, `Application.MainPage`), XAML (`*AndExpand`, FontSize NamedSize), Core+Behaviors nullable, formatter config fix (`useTabs:false`). Remaining: central build, VM nullable cascade mop-up, dead-field removal, verify/commit. See [Phase-4-Migration.md](./Phase-4-Migration.md). (Real deduped counts were ~¼ of the raw Phase-2 estimates.)
- [ ] **Phase 6** — break up `VehiclesViewModelBase` (1,318 LOC) + `AuslassLogic.Auspuff` (460-LOC method); fix duplicate popup registration; standardize source generators.
- [ ] **Phase 8** — `DatabaseContext` Singleton→Scoped; remove 22 `Ioc.Default` View call-sites; decouple `SimTuning.Data` from `Microsoft.Maui.Storage`.
- [ ] **Phase 9** — drop Serilog `Verbose` floor in Release.
- [ ] **Phase 14** — zip-entry validation + System.Text.Json; path canonicalization; encrypt-at-rest decision; remove `RNGCryptoServiceProvider`/`SecureString`.
- [ ] **Phase 15** — add `xunit.runner.visualstudio`; real assertions; fix dead tests (`AudioLogicTest` missing `[Fact]`, `DynoLogicTest` commented); cross-platform test paths; async tests.
- [ ] **Phase 17** — replace vendored `Spectrogram` with NuGet (sole consumer `AudioLogic` confirmed; dead render/SFF/Colormap surface documented).

Full detail and rationale: see [Phase-1-Analysis.md](./Phase-1-Analysis.md) §6–§9.
