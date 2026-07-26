# SimTuning Modernization — Living Docs

Continuous documentation for the .NET 10 → .NET 11 modernization (branch `v2.0.0`).
Per the roadmap, docs are **seeded in Phase 1 and refreshed every phase** — not written at the end.

## Index

| Doc | Purpose |
|---|---|
| [Phase-1-Analysis.md](./Phase-1-Analysis.md) | Full project analysis: structure, per-project findings, security, .NET 11 risk register, TODO → phases |
| [Phase-2-Migration.md](./Phase-2-Migration.md) | .NET 11 TFM migration log: changes, decisions, verification, environment setup |
| [Phase-3-Migration.md](./Phase-3-Migration.md) | NuGet package upgrade log: version decisions, Popup v2 migration, NU1903 CVE clearance |
| [Phase-4-Migration.md](./Phase-4-Migration.md) | Fix-compilation log: obsolete APIs, XAML deprecations, nullable-reference migration (484→24 warnings; production clean) |
| [Phase-5-Migration.md](./Phase-5-Migration.md) | Modern-C# log: using-declarations applied; file-scoped namespaces / record / global-usings scoped/deferred with rationale |
| [Phase-6-Migration.md](./Phase-6-Migration.md) | Structural log: popup-reg fix (Phase-1 direction corrected); `AuslassLogic.Auspuff` extracted; `VehiclesViewModelBase` split + mirrors collapsed; 24 commands → `[RelayCommand]` |

## Phase 2 — ✅ Complete (migrate TFMs to .NET 11)

**Outcome:** all 6 projects build on .NET 11 (`11.0.100-preview.6`). The full **MacCatalyst desktop app builds green**, and every library compiles on `net11.0`, `net11.0-ios`, and `net11.0-maccatalyst`. The only remaining build failure is `SimTuning.Maui.App` on `net11.0-ios`, blocked by a **local iOS simulator-runtime mismatch** (environmental, pre-existing — the README already documents iOS simulator issues), fixable with `xcodebuild -downloadPlatform iOS`.

**Beyond the brief's scope (justified):** the migration exposed that `SimTuning.Test` inherited mobile TFMs (so it was being built as an iOS/Mac *app bundle*). The proper fix — pin Test to `net11.0`-only and give `Core`/`Maui.UI`/`Spectrogram` a `net11.0` base target — was done here rather than deferred, because it's required for the solution to build and is the conventional MAUI test setup. Test *contents* (assertions, cross-platform paths, missing `xunit.runner.visualstudio` adapter) remain Phase 15.

See [Phase-2-Migration.md](./Phase-2-Migration.md) for the full change list and decisions.

## Phase 3 — ✅ Complete (upgrade NuGet packages)

**Outcome:** all packages upgraded to newest stable (preview only where .NET 11 requires). **NU1903 vulnerable-package warnings cleared (80 → 0)** via the EF Core 11 bump. The full MacCatalyst app builds green with all upgraded packages; zero code/package errors remain (only the pre-existing iOS simulator-runtime env block).

**Highlights:** Maui.Controls→`11.0.0-preview.6`, EF Core→`11.0.0-preview.6`, CommunityToolkit.Maui→`15.0.0` (the .NET 11 compat release — drove a **Popup v2 migration** on 4 popups), MediaElement→`10.0.0` (new required `isAndroidForegroundServiceEnabled` arg), LiveCharts `rc4.5`→`2.0.5` (stable!), FftSharp→`2.2.0`. Removed unused `Maui.Controls.Compatibility`; made `WinUI.Notifications` Windows-conditional. Kept SkiaSharp 3.119.1 (LiveCharts pins it), Sharpnado (no net11 build), NAudio 2.2.1 (Windows-only — replacement plan documented).

See [Phase-3-Migration.md](./Phase-3-Migration.md) for the full version table and decisions.

## Phase 4 — ✅ Complete (fix compilation/analyzer warnings)

**Outcome:** unique solution warnings **484 → 24**, **0 non-iOS errors** (MacCatalyst builds green). **Production code is warning-clean** — the 24 remaining are all mapped to later phases. See [Phase-4-Migration.md](./Phase-4-Migration.md).

**Highlights:** SkiaSharp `SKPaint`→`SKFont` migration (CS0618 ×22); removed dead `RNGCryptoServiceProvider` (SYSLIB0023); `Application.MainPage`→`Windows`; XAML `*AndExpand`→base (×173) and `FontSize` NamedSize→explicit doubles (×21); nullable-reference migration across Core + all ViewModels (the bulk: CS86xx family). Formatter config fix: **`.csharpierrc.json useTabs:true→false`** (the codebase is space-indented; `useTabs:true` caused a 362-warning SA1027 flood). ~10 justified `!` at null-tolerant boundaries, each commented.

**Deferred (the 24):** ListView→CollectionView CS0618 ×6 (P6/12), MAUIG2045 binding hints ×3 (P6), AsyncFixer02 sync-IO ×2 (P7), test CS8625/CS8602/xUnit1008 ×11 (P15), dead fields ×2 (P16).

⚠️ **Gotchas (see Phase-4 doc):** keep `.csharpierrc useTabs:false`; NEVER recursive `csharpier format` (phantom prettier on ~70 files); `dotnet` not on PATH; iOS build error is environmental.

⚠️ **Visual review needed on device:** the `*AndExpand`→base XAML migration may shift layouts with extra space; the FontSize NamedSize→explicit-double change is a minor size delta vs the old platform-dynamic values.

## Phase 5 — ✅ Complete (use modern C#)

**Outcome:** deliberately scoped & low-churn. The codebase already has Nullable + ImplicitUsings + LangVersion=latest, and Phase 4 applied modern nullable idioms — so the remaining surface is small. See [Phase-5-Migration.md](./Phase-5-Migration.md).

**Applied:** `using` declarations — `BrowserService.DownloadDocumentAsync` (3 nested blocks → 3 `using var`) and `VehicleService.DeleteOne(VehiclesModel)` (block → `using var`). Identical disposal scope, less nesting, no behavior change.

**Deferred (with rationale):** file-scoped namespaces (180-file churn conflicting with P6/P12 → converge per-file); `global usings` (ImplicitUsings already on); `record`/`init`/`required` (EF entities + framework-message subclasses); target-typed `new()` (SA1000 blocks it); structural modernization (→ P6/P12).

## Phase 6 — ✅ Complete (structural modernization)

**Outcome:** all four mandated sub-tasks done, **behavior-preserving**; MacCatalyst builds green, **0 errors, 0 new warnings** (warning-neutral vs HEAD). See [Phase-6-Migration.md](./Phase-6-Migration.md).

**Highlights:**
- **Popup fix + Phase-1 correction:** CommunityToolkit `PopupService` maps VM→view with `TryAdd` (**first registration wins**), so `DynoCreationPopup`/`EnvironmentPopup` were the *dead* pair (Phase-1 had the direction backwards) — deleted; `VehiclePopup`/`PortTimingPopup` kept. Phase-1 doc corrected.
- **`AuslassLogic.Auspuff`** (~460-LOC method) → `Calculate` + `ComputeGeometry` (returns an `AuspuffGeometry` record) + 4 `Draw*` helpers; the triply-nested stage `if/else` (duplicated across 3 regions) deduped to computed `HasD1/2/3` flags. Identical draw calls.
- **`VehiclesViewModelBase`** (1,318 LOC) → 6 partial files; ~60 mirror props collapsed via one `SetMirror<TParent>` helper; the 70-line `raiseAllPropertyChanged()` → `OnPropertyChanged(string.Empty)`. Public surface **byte-identical** (69 members, empty before/after diff).
- **`[RelayCommand]`:** 24 hand-written commands across 9 VMs converted to source-generated commands; `StartBeschleunigung`→`StartAcceleration` and `ResetRun`→`ResetAcceleration` (no callers) to preserve the XAML-bound command names. All 26 bound `*Command` names still resolve.

⚠️ **Corrections surfaced:** (1) Phase-1 popup shadow direction was wrong (first-wins, not last-wins) — fixed in `Phase-1-Analysis.md`. (2) A U+FEFF a Plan agent flagged as "load-bearing" on `VehicleMotorDeachsierungL` was proven (hexdump) to be plain-ASCII on the property declaration (it lived only inside the now-deleted `nameof`) — **no latent XAML binding bug**. (3) The Auspuff extraction introduced 24 false-positive CS8629 (splitting calc/geometry across methods lost HEAD's intra-method non-null proof); suppressed locally with a documented pragma — identical cast IL.

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
- [x] **Phase 4** — ✅ done. 484→24 unique warnings; production code warning-clean; 0 non-iOS errors. Fixed: obsolete APIs (SkiaSharp `SKFont`, RNGCryptoServiceProvider, `Application.MainPage`), XAML (`*AndExpand`, FontSize NamedSize), nullable migration (Core + all VMs), formatter config (`useTabs:false`). 24 remaining all deferred to P6/7/15/16. **Carried forward:** device visual review of AndExpand/FontSize changes; `dotnet test` still 0-discovered (adapter missing, P15); cspell not a committed devDep.
- [x] **Phase 5** — ✅ done (focused). `using` declarations (BrowserService, VehicleService). Deferred w/ rationale: file-scoped namespaces (converge per-file), record/init/required (not applicable), target-typed new (SA1000), structural (→P6/P12).
- [x] **Phase 6** — ✅ done. Broke up `VehiclesViewModelBase` (→ 6 partials, ~60 mirrors collapsed via `SetMirror`, `raiseAllPropertyChanged`→`OnPropertyChanged(string.Empty)`) and `AuslassLogic.Auspuff` (→ `Calculate`/`ComputeGeometry`/`Draw*` + `AuspuffGeometry` record); fixed duplicate popup reg (deleted dead `DynoCreationPopup`/`EnvironmentPopup` — Phase-1 direction corrected to first-wins); standardized 24 commands → `[RelayCommand]`. **0 errors, 0 new warnings**; public-surface + command-name invariants verified. **Carried forward:** device smoke test; AuslassLogic CS8629 pragma removal (P12); `[ObservableProperty]` sweep (P12).
- [ ] **Phase 8** — `DatabaseContext` Singleton→Scoped; remove 22 `Ioc.Default` View call-sites; decouple `SimTuning.Data` from `Microsoft.Maui.Storage`.
- [ ] **Phase 9** — drop Serilog `Verbose` floor in Release.
- [ ] **Phase 14** — zip-entry validation + System.Text.Json; path canonicalization; encrypt-at-rest decision; remove `RNGCryptoServiceProvider`/`SecureString`.
- [ ] **Phase 15** — add `xunit.runner.visualstudio`; real assertions; fix dead tests (`AudioLogicTest` missing `[Fact]`, `DynoLogicTest` commented); cross-platform test paths; async tests.
- [ ] **Phase 17** — replace vendored `Spectrogram` with NuGet (sole consumer `AudioLogic` confirmed; dead render/SFF/Colormap surface documented).

Full detail and rationale: see [Phase-1-Analysis.md](./Phase-1-Analysis.md) §6–§9.
