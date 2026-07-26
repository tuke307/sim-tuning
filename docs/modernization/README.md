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
| [Phase-7-Migration.md](./Phase-7-Migration.md) | Async log: AsyncFixer02 sync-IO cleared (2→0); `System.Timers.Timer`→`IDispatcherTimer`; `ShowSnackbarDialog` `async void`→`Task`; NavigationService handlers hardened; `CancellationToken` on download |
| [Phase-8-Migration.md](./Phase-8-Migration.md) | DI log: `DatabaseContext`→`AddDbContextFactory` + `IDbContextFactory` in `VehicleService` (replaces dead Singleton + 15 `new`); View `Ioc.Default` + Data↔MAUI.Storage decoupling deferred w/ rationale |
| [Phase-9-Migration.md](./Phase-9-Migration.md) | Logging log: Serilog `Verbose` floor → per-config (`#if DEBUG` Debug/`#else` Warning); 3 message-as-template sites fixed |
| [Phase-11-Migration.md](./Phase-11-Migration.md) | Exceptions log: `throw new Exception()`→`InvalidOperationException` (NavigationService §7.7); 3 bare `catch`→`catch (Exception)` (DatabaseSettings); SFF/AudioUtlis catches deferred to P13/P17 |
| [Phase-12-Migration.md](./Phase-12-Migration.md) | DRY log: 45 UnitsNet `*Unit` setters → `ConvertValueForUnit` helper in `BaseEntityModel` (−737 LOC; AuspuffModel 1047→~670); 0 errors, 0 new warnings |
| [Phase-17-Migration.md](./Phase-17-Migration.md) | Spectrogram log: NuGet rejected (Windows-only/unmaintained); vendored copy trimmed 995→318 LOC (deleted SFF/Image/Colormap/Tools; trimmed Spectrogram.cs; dropped AllowUnsafeBlocks + SkiaSharp; AudioLogicTest repointed to GetFFTs) |
| [Phase-14-Migration.md](./Phase-14-Migration.md) | Security log: Zip-Slip defense in ImportDyno (A01); dead `SecureString` methods removed (A02); Newtonsoft→System.Text.Json + dependency dropped (A08); path canonicalization + encrypt-at-rest deferred |
| [Phase-16-Migration.md](./Phase-16-Migration.md) | Dead-code log: deleted Tizen platform + all-stub `TuningLogic` + commented blocks in ImportDyno/`DynoRuntimeViewModel` (OnLocationUpdated/StartRecording); ~150 LOC removed. **Completion pass:** removed 2 dangling `StartRecording` comments + the 2 Phase-4-deferred dead fields (`_locationService`/`trackingStarted`) — baseline 12→10 |
| [Phase-15-Migration.md](./Phase-15-Migration.md) | Test log: `xunit.runner.visualstudio` + coverlet added (in `2ddc1fc`); `dotnet test` 0-discovered → **47 discovered, 46 passed / 1 skipped / 0 failed**; fixed `AudioLogicTest(65536)` window count; skipped `DynoAudio` (→P8b); redesigned `DynoRuntime` integration path; deleted dead `IViewModelTest` |

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

## Phase 7 — ✅ Complete (async/await modernization)

**Outcome:** AsyncFixer02 sync-IO **2 → 0** (unique warnings **14 → 12**); MacCatalyst builds green, **0 errors, 0 new warnings** (no AsyncFixer03/04 or CS4014 introduced). See [Phase-7-Migration.md](./Phase-7-Migration.md).

**Highlights:**
- **AsyncFixer02 cleared** — `DynoDataViewModel`: `File.WriteAllText`→`WriteAllTextAsync`, `ZipFile.ExtractToDirectory`→`ExtractToDirectoryAsync` (the 2 warnings Phase 4 deferred to P7). Security hardening of `ImportDyno` stays Phase 14.
- **`DynoRuntimeViewModel` timer → `IDispatcherTimer`** — `System.Timers.Timer.Elapsed` fired on a thread-pool thread and mutated bound properties off-thread; the UI-thread `IDispatcherTimer` fixes that. `CreateDispatcherTimer` helper (via `Application.Current.Dispatcher.CreateTimer()`); `.Dispose()` calls removed (`IDispatcherTimer` isn't `IDisposable`); handler sigs `ElapsedEventArgs`→`EventArgs`. No constructor change → test still compiles.
- **`Functions.ShowSnackbarDialog` `async void` → `async Task`** (`ShowSnackbarDialogAsync`) — the one non-event-handler async void; 9 call sites → `_ =` fire-and-forget (preserves semantics, suppresses CS4014, no AsyncFixer04).
- **NavigationService `async void` handlers hardened** — `Page_Navigated{To,From}` wrapped in try/catch + logged (injected `ILogger<NavigationService>`); an unhandled throw no longer crashes the process.
- **`CancellationToken`** — `IBrowserService.DownloadDocumentAsync` (+ impl) takes an optional `CancellationToken` (flowed to `GetStreamAsync`/`CopyToAsync`); caller unchanged.

⚠️ **Deferred (with rationale):** `DynoAudioViewModel` CPU offload (needs thread-marshaling + `AudioLogic` static-state thread-safety; no tests yet → Phase-15 follow-up); popup `Button_Clicked` try/catch (Phase 11); `ConfigureAwait(false)` sweep (AsyncFixer03 not firing; MAUI guidance says *don't* add it in VMs).

## Phase 8 — 🔶 Partial (DI / DbContext lifetime)

**Outcome:** sub-task 1 (the Phase-1 "standout bug") done, behavior-preserving, build-verified; sub-tasks 2 & 3 deferred with precise technical rationale. MacCatalyst green, **0 errors, 0 new warnings.** See [Phase-8-Migration.md](./Phase-8-Migration.md).

**Done — `DatabaseContext` lifetime:** `AddSingleton<DatabaseContext>()` → `AddDbContextFactory<DatabaseContext>()`; `VehicleService` injects `IDbContextFactory<DatabaseContext>`; the 15 `new DatabaseContext()` → `_dbFactory.CreateDbContext()` (still short-lived `using var` per op). `VehicleService` stays Singleton (its in-memory caches are Singleton-dependent → behavior identical); the factory (not a Scoped context) is the captured Singleton dep → **no captive-dependency**. The old Singleton DbContext reg was dead anyway (nobody resolved it — `VehicleService` always `new`'d its own); the real smell was the service-locator `new`, now gone. `IVehicleService` unchanged → mock tests unaffected.

⚠️ **Deferred (with rationale):**
- **23 `Ioc.Default` View sites** — `MainPage` is a `Shell` with `ShellContent ContentTemplate="{DataTemplate …View}"`; Shell's DataTemplate needs **parameterless ctors**, so constructor injection requires a Shell→registered-routes navigation restructure + device verification (none available). → follow-up.
- **Data ↔ `Microsoft.Maui.Storage` decoupling** — wide static→instance ripple on `DatabaseSettings` (consumed across Data/Core/UI + inside `OnConfiguring`); Phase-1 P3. → follow-up (`IPlatformSettings` abstraction).

## Phase 9 — ✅ Complete (logging posture)

**Outcome:** Serilog `Verbose` floor dropped (Phase-1 A05); per-configuration level (`#if DEBUG` `Debug` / `#else` `Warning`); 3 message-as-template sites fixed. MacCatalyst green in **both Debug and Release**, **0 errors, 0 new warnings.** See [Phase-9-Migration.md](./Phase-9-Migration.md).

**Highlights:**
- **`SetupSerilog`** — `MinimumLevel.Verbose()` → `MinimumLevel.Is(minimumLevel)` with `#if DEBUG Debug #else Warning`. Verified **nothing logs at Verbose/Trace**, so no app output is lost; Release now logs only `Warning`/`Error` (minimizes PII — `Information` events carry dyno/vehicle names — and log volume). `Microsoft→Warning` override + Debug/File sinks unchanged.
- **Structured logging** — 3 `_logger.LogXxx(exc, exc.Message)` sites (VehicleService RetrieveDynos/RetrieveMotoren, BrowserService OpenBrowser) → fixed templates (matching the existing sibling pattern). No `{0}` positional templates existed.

## Phase 11 — ✅ Complete (exception handling)

**Outcome:** the untyped `throw new Exception()` (Phase-1 §7.7) → `InvalidOperationException` with a message; 3 bare `catch { }` → explicit `catch (Exception)` (design-time fallbacks preserved). MacCatalyst green, **0 errors, 0 new warnings.** See [Phase-11-Migration.md](./Phase-11-Migration.md).

**Highlights:**
- **`NavigationService.Navigation`** getter — `throw new Exception();` → `throw new InvalidOperationException("No active navigation stack is available.")`. Same propagation; better type + message.
- **`DatabaseSettings`** — 3 bare `catch` (GetAppDataDirectory/GetPreference/SetPreference) → `catch (Exception)` (idiomatic explicit filter; comments + safe-default behavior unchanged).
- **VM/service catches left as-is** — already proper (`catch (Exception exc) + LogError(exc, "... {Message}", exc.Message)`; `{Message}` is a named template + arg, not the Phase-9 anti-pattern). Narrowing them risks behavior change for no clear gain.
- **Deferred:** `SFF.cs:101` empty catch (vendored Spectrogram → P17); `AudioUtlis.cs` empty catches (NAudio → P13).

## Phase 12 — ✅ Complete (DRY UnitsNet `*Unit` boilerplate)

**Outcome:** the ~46× duplicated unit-conversion setter boilerplate collapsed into one `ConvertValueForUnit` helper in `BaseEntityModel`. **811 deletions / 74 insertions (net −737 LOC);** MacCatalyst green, **0 errors, 0 new warnings;** Test compiles. See [Phase-12-Migration.md](./Phase-12-Migration.md).

**Highlights:**
- **`ConvertValueForUnit(double?, Enum, Enum)`** in `BaseEntityModel` — reproduces the inlined `UnitConverter.TryConvert` + `RoundOnUnitChange` logic verbatim. `Enum` params (not a generic) so the nullable `XUnit?` types box exactly as before; the underlying `TryConvert` call is byte-identical.
- **45 live setters** across 8 model files collapsed (~13 lines → ~2 each) via a conservative regex script (backreferences enforce same prop name; 1 commented-out `FrontA` stub correctly skipped). `AuspuffModel` 1,047 → ~670. Conversion/rounding policy now centralized (one place, was 45).
- **Behavior-preserving** — same `TryConvert` call + policy + `!HasValue` path.

⚠️ **Deferred (with rationale):** AuslassLogic CS8629 pragma removal (non-trivial non-null propagation); `[ObservableProperty]` sweep (limited applicability — mirrors delegate to model, no backing field); `EnsureDatabaseCreated` `Migrate()`+`EnsureCreated()` (DB-init behavior change, needs device); `Styles_Global` ListView→CollectionView (XAML, needs device visual review).

## Phase 17 — ✅ Complete (vendored Spectrogram cleanup)

**Outcome:** the modern `Spectrogram` NuGet was **evaluated and rejected** (depends on `System.Drawing.Common` → Windows-only, image-focused with no `GetFFTs()` raw-data API, unmaintained since 2022). Per the brief's fallback, the vendored copy was **kept and trimmed: 995 → 318 LOC (−677).** MacCatalyst green, **0 errors, 0 new warnings;** Test compiles. See [Phase-17-Migration.md](./Phase-17-Migration.md).

**Highlights:**
- **Deleted 4 dead files** (534 LOC): `SFF.cs` (297 — the untrusted-binary `.sff` reader/writer + empty catch), `Image.cs` (119 — bitmaps), `Tools.cs` (90 — MIDI/SFF helpers), `Colormap.cs` (28). Verified 0 external refs (the `Image.`/`Tools.` grep hits were `SKImage`/`System.Resources.Tools` false positives).
- **`Spectrogram.cs` trimmed 266→~110** — kept only the consumed surface (ctor, `Add`, `Process`, `GetFFTs`, `HzPerPx`/`SecPerPx`); the `Process` FFT core is byte-identical, so consumed behavior is preserved.
- **Modernized:** removed `AllowUnsafeBlocks` (unused), dropped `SkiaSharp` from the code, stripped 10 `Console.WriteLine` debug lines from `WavFile`, repointed `AudioLogicTest` to `GetFFTs()`.
- `AudioLogic` is the sole consumer; its API (`WavFile.ReadMono`, ctor/`Add`/`HzPerPx`/`SecPerPx`/`GetFFTs`) is unchanged.

⚠️ **Carry-forward:** optional rewrite of `AudioLogic` directly on `FftSharp` to drop the vendored project entirely (needs an audio-equivalence test); `AudioLogicTest` activation (`[Theory]` + cross-platform path) → Phase 15.

## Phase 14 — 🔶 Partial (security hardening)

**Outcome:** Zip-Slip defense added (A01), dead deprecated `SecureString` removed (A02), `Newtonsoft.Json`→`System.Text.Json` + dependency dropped (A08). MacCatalyst green, **0 errors, 0 new warnings.** Path canonicalization + encrypt-at-rest deferred. See [Phase-14-Migration.md](./Phase-14-Migration.md).

**Highlights:**
- **Zip-Slip defense (A01)** — `ImportDyno` now opens the archive and rejects any entry whose canonicalized path escapes `FileDirectory` (was `ExtractToDirectoryAsync` with no per-entry check). Localized `AsyncFixer02` pragma (per-entry validation needs sync `OpenRead`); `overwrite:true`.
- **`SecureString` removal (A02)** — `Converts.SecureStringToString`/`StringToSecureString` deleted (0 refs, deprecated); `SKBitmapToStream` kept. (`RNGCryptoServiceProvider` already gone in P4; `SFF` untrusted-binary parse gone via P17.)
- **Newtonsoft→System.Text.Json (A08)** — `DynoDataViewModel` serialize (active) + deserialize (commented) → `JsonSerializer` with `ReferenceHandler.Preserve` (≈ `PreserveReferencesHandling.Objects`); **`Newtonsoft.Json` package removed** from `Directory.Build.props`. ⚠️ Export JSON format changed (no code round-trips it today).
- **2 transient StyleCop warnings** (SA1000/SA1204 from target-typed `new()`) caught + fixed.

⚠️ **Deferred:** path canonicalization (intertwined with Phase-8b static-settings decoupling); **encrypt-at-rest** (needs a threat-model + key-management decision — does the team want AES-at-rest on the SQLite, and where's the key?).

## Phase 16 — ✅ Complete (dead-code deletion)

**Outcome:** deleted the Tizen platform, the all-stub `TuningLogic`, and the large commented-out blocks in `DynoDataViewModel.ImportDyno` + `DynoRuntimeViewModel` (`OnLocationUpdated`, `StartRecording`). **~150 LOC of dead code removed.** MacCatalyst green, **0 errors, 0 new warnings;** Test compiles. See [Phase-16-Migration.md](./Phase-16-Migration.md).

**Highlights:**
- **Tizen** — `Platforms/Tizen/` deleted (TFM was commented out → never built).
- **`TuningLogic`** — deleted (3 all-TODO-stub methods, 0 refs in prod/tests).
- **Commented blocks** — `ImportDyno` tail cleaned (ends after the validated extraction); `DynoRuntimeViewModel.OnLocationUpdated` (~53 commented lines) + `StartRecording` removed (both unreferenced).
- Spectrogram dead surface already removed in P17.

⚠️ **Deferred:** broader commented-code sweep across VMs/Logic (scattered/cosmetic); `IViewModelTest` empty contract → P15.

> **Completion pass:** closed the two carry-forward items (the dangling `//await StartRecording()` comments) **and** the Phase-4 "dead fields ×2" deferral (`_locationService` CS0169 + `trackingStarted` CS0414 in `DynoRuntimeViewModel` — orphaned when the original pass deleted their consumers). Warning baseline **12 → 10**. `IViewModelTest` resolved in Phase 15.

## Phase 15 — ✅ Complete (test project modernization)

**Outcome:** `dotnet test` went from **0-discovered** (the Phase-4 state — the adapter was missing) to **47 tests discovered: 46 passed, 1 skipped, 0 failed**, test-project warning-clean. The file-by-file modernization (adapter + coverlet, real assertions, cross-platform temp paths, the missing `[Theory]` on `AudioLogicTest`, the commented `DynoLogicTest` body) landed in `2ddc1fc`; this phase **resolved the 3 failures those real assertions surfaced**. See [Phase-15-Migration.md](./Phase-15-Migration.md).

**Highlights:**
- **`AudioLogicTest(65536)`** — the 1-second @ 44.1 kHz test WAV (44 100 samples) was shorter than the 65 536-sample FFT window → `GetFFTs()` empty. Scaled the generated duration with `fftSize` (≥2 windows per size). Test-data fix; `AudioLogic` untouched.
- **`DynoAudioViewModelTest`** — `FilterPlot`→`CheckDynoData` hits `GeneralSettings.AudioAccelerationFilePath` → `Preferences.Default.Get` with **no design-time fallback** (unlike `DatabaseSettings`). That's the Phase-8b Data↔MAUI.Storage coupling → **skipped with a pointer to P8b** (re-enabled when `GeneralSettings` gets fallback parity).
- **`DynoRuntimeViewModelTest`** — `StartAcceleration` is an integration-only path (permissions via `Functions.GetPermission` + `IDispatcherTimer` via `Application.Current.Dispatcher`, neither present in a headless host). Redesigned to construct + `ResetAccelerationCommand` only; the run flow is on-device.
- **Deleted `IViewModelTest`** (empty marker interface — the Phase-16 carry-forward) and **fixed the last test warning** (`NewDyno(null)` CS8625 → `new VehiclesModel()`).

⚠️ **Carry-forward:** re-enable `DynoAudioViewModelTest` (→ Phase 8b); device smoke test for the `DynoRuntime` permission/run flow.

⚠️ **Test-only changes** — the MacCatalyst head and its 12-warning baseline are untouched.

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
- [x] **Phase 7** — ✅ done. AsyncFixer02 sync-IO cleared (2→0; unique warnings 14→12); `DynoRuntimeViewModel` `System.Timers.Timer`→`IDispatcherTimer` (cross-thread fix); `ShowSnackbarDialog` `async void`→`Task` (9 call sites `_ =`); `NavigationService` async-void handlers try/catch+log; `CancellationToken` on `DownloadDocumentAsync`. **0 errors, 0 new warnings.** **Carried forward:** device smoke test (dyno run + import/export); `DynoAudioViewModel` CPU offload (after AudioLogic thread-safety + P15 tests); popup `async void` try/catch (P11).
- [~] **Phase 8** — 🔶 partial. **Done:** `DatabaseContext`→`AddDbContextFactory` + `IDbContextFactory` injected into `VehicleService` (replaces dead Singleton DbContext reg + 15 `new DatabaseContext()` service-locator calls; `VehicleService` kept Singleton → cache behavior identical; no captive dep). **0 errors, 0 new warnings.** **Deferred w/ rationale:** (a) 23 `Ioc.Default` View sites — Shell `ContentTemplate` DataTemplate needs parameterless ctors → requires Shell→registered-routes nav restructure + device verification; (b) Data↔`Microsoft.Maui.Storage` decoupling — wide static→instance `DatabaseSettings` ripple incl. `OnConfiguring` (P3). **Carried forward:** CRUD device smoke test; `EnsureDatabaseCreated` `Migrate()`+`EnsureCreated()` anti-pattern (P12).
- [ ] **Phase 8b (follow-up)** — View `Ioc.Default` removal (Shell→registered-routes) + Data `IPlatformSettings` decoupling — needs device verification.
- [x] **Phase 9** — ✅ done. Serilog `Verbose` floor → per-config (`#if DEBUG` `Debug`/`#else` `Warning`); 3 message-as-template sites fixed (VehicleService ×2, BrowserService ×1). **0 errors, 0 new warnings; green in Debug + Release.** **Carried forward:** optional `LoggingLevelSwitch`; optional PII redaction if Release surface broadened.
- [x] **Phase 11** — ✅ done. `throw new Exception()`→`InvalidOperationException` (NavigationService §7.7, +message); 3 bare `catch`→`catch (Exception)` (DatabaseSettings design-time fallbacks). VM/service catches already proper (left as-is). **0 errors, 0 new warnings.** **Carried forward:** SFF empty catch (P17); AudioUtlis empty catches (P13).
- [x] **Phase 12** — ✅ done. 45 UnitsNet `*Unit` setters → `ConvertValueForUnit` helper in `BaseEntityModel` (−737 LOC; AuspuffModel 1047→~670); 0 errors, 0 new warnings; Test compiles. **Carried forward:** AuslassLogic CS8629 pragma (non-null propagation); `[ObservableProperty]` sweep (limited applicability); `EnsureDatabaseCreated` Migrate+EnsureCreated (needs device); Styles_Global ListView→CollectionView (needs device).
- [~] **Phase 14** — 🔶 partial. **Done:** Zip-Slip defense in `ImportDyno` (A01, per-entry path validation); dead `SecureString` methods removed (A02); `Newtonsoft.Json`→`System.Text.Json` + package dropped (A08). **0 errors, 0 new warnings.** **Deferred w/ rationale:** path canonicalization (→ Phase 8b static-settings decoupling); **encrypt-at-rest decision** (needs threat-model + key-management — surface to user). `RNGCryptoServiceProvider`/`SFF` already gone (P4/P17). **Carried forward:** Export→Import device smoke test.
- [x] **Phase 15** — ✅ done. `xunit.runner.visualstudio` + coverlet added (in `2ddc1fc`) → `dotnet test` **0-discovered → 47 discovered (46 passed / 1 skipped / 0 failed)**, test-project warning-clean. Resolved the 3 failures the new real assertions surfaced: `AudioLogicTest(65536)` WAV-duration window-count fix; `DynoAudioViewModelTest` skipped (→ P8b `GeneralSettings` Preferences fallback); `DynoRuntimeViewModelTest` redesigned (drop the integration-only `StartAcceleration` path). Deleted dead `IViewModelTest`; fixed last test warning (`NewDyno(null)` CS8625). **Test-only — head + 12-warning baseline untouched.** **Carried forward:** re-enable `DynoAudioViewModelTest` (P8b); `DynoRuntime` run-flow device smoke test.
- [x] **Phase 17** — ✅ done. Spectrogram NuGet **rejected** (Windows-only `System.Drawing.Common`, image-focused, unmaintained); vendored copy **trimmed 995→318 LOC** (deleted SFF/Image/Colormap/Tools = −534; `Spectrogram.cs` 266→~110; dropped `AllowUnsafeBlocks` + SkiaSharp; `WavFile` console-spam removed; `AudioLogicTest` → `GetFFTs`). Consumed API + `Process` FFT core unchanged. **0 errors, 0 new warnings;** Test compiles. **Carried forward:** optional FftSharp-direct rewrite of AudioLogic (needs audio-equivalence test); AudioLogicTest activation (P15).

Full detail and rationale: see [Phase-1-Analysis.md](./Phase-1-Analysis.md) §6–§9.
