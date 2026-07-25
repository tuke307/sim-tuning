# Phase 1 — Project Analysis Report

> SimTuning modernization, branch `v2.0.0` (HEAD `36abfe1`). Analysis only — **no code changed**.
> Generated 2026-07-25. Graphify graph from `e0ecdbae` (current enough; 3 newer commits are security/APM/gitignore only).
> Source: graphify orientation + 4 parallel per-layer analysis passes (Core+Spectrogram, Data, Maui.UI, Maui.App+Test) + baseline build attempt.

---

## 0. Executive summary

SimTuning is a .NET MAUI multi-platform app (German UI `de-DE`, v2.0.0) — a Simson two-stroke tuning tool: engine/exhaust/intake calculations, a vehicle/dyno data layer (EF Core + SQLite), dyno run recording, and audio spectrogram analysis. Six projects, ~197 files / ~95k words. Architecture is sound and conventional (CommunityToolkit.Mvvm + MAUI DI + Serilog + EF Core), and the code **compiles cleanly** in principle — but the **local environment cannot build it today** (missing workloads), and there is significant accrued technical debt concentrated in a few god-classes and one systemic anti-pattern (service-locator in Views).

**Headline risks (full detail in §6–§8):**
1. **Environment blockers** — no MAUI workloads installed (build fails); only the `11.0.100-preview.6` SDK is present (no .NET 10 side-by-side, contrary to the migration brief).
2. **Cross-platform audio blocker** — `NAudio 2.2.1` (used in `Core/Helpers/AudioUtils`) is Windows-only; will fail at runtime on iOS/Mac/Android.
3. **God classes** — `VehiclesViewModelBase` (1,318 LOC), `AuslassLogic.Auspuff` (~460 LOC method), `AuslassAnwendungViewModel` (752), `AuspuffModel` (1,047, ~90% boilerplate).
4. **Functional bugs** (not just modernization) — iOS `Info.plist` missing mic/location usage strings (runtime crash + App Store rejection); duplicate popup→VM registration (`VehiclePopup`/`PortTimingPopup` unreachable); `DatabaseContext` registered as Singleton.
5. **Test quality** — tests are smoke-tests only (no assertions on outputs); one test missing `[Fact]` (silently skipped); `DynoLogicTest` body commented out; hardcoded `C:\Users\Tony\…` path.

---

## 1. Corrected assumptions (the migration brief was wrong on three points)

| Brief assumed | Reality on this machine | Impact |
|---|---|---|
| .NET 10 SDK installed **side-by-side** with the preview | **Only** `11.0.100-preview.6.26359.118` is installed; no .NET 10 SDK | Old `net10.0-*` TFMs cannot be built/validated locally either; SDK rollforward carries the build. Phase 2 should still pin a `global.json`. |
| `sudo dotnet workload restore` brings iOS/Mac workloads | **No workloads installed at all** (`dotnet workload list` empty) | **Build currently fails** (`NETSDK1147`: needs `maui-ios`). Workload restore is a hard Phase-2 prerequisite. |
| `SimTuning.Test` is `net10.0`-only | `SimTuning.Test.csproj` declares **no** `<TargetFramework>` → inherits `net10.0-ios;net10.0-maccatalyst` | The test project pulls the iOS workload (it appears in the workload error list). Should be pinned `net10.0`-only with `UseMaui=false`. |

---

## 2. Solution structure

```
SimTuning.sln  (src/)
├── SimTuning.Maui.App   (Exe, MAUI host)   Platforms/{Windows,iOS,MacCatalyst,Android,Tizen}, MauiProgram.cs, App.xaml.cs
├── SimTuning.Maui.UI    (library)          Views/ + ViewModels/ (feature folders: Dyno, Motor, Einlass, Auslass, Home, Popups), Services/, Behaviors/, Themes/, Styles/
├── SimTuning.Core       (library)          Models/ (incl. Quantity/), Services/, ModuleLogic/ (calc), Helpers/, Converters/
├── SimTuning.Data       (library, net10.0-only, UseMaui=false, Nullable=disable)  DatabaseContext, Models/, Migrations/
├── Spectrogram          (vendored source ~6yr old)  Spectrogram.cs, WavFile.cs, SFF.cs, Image.cs, Colormap.cs, Tools.cs, Settings.cs
└── SimTuning.Test       (xUnit + Moq)      ModuleLogic/, ViewModels/
```

**Dependency direction:** `Maui.App → Maui.UI → Core → {Data, Spectrogram}`; `Test → {Core, Maui.UI}`. All libraries except `Data` inherit the multi-TFM `net10.0-ios;net10.0-maccatalyst[;net10.0-windows10.0.19041.0]` and shared package versions from `src/Directory.Build.props`.

**Approximate size:** Core ~3,265 LOC · Data ~6,200 LOC (mostly UnitsNet boilerplate) · Maui.UI ~7,460 LOC VMs + 542 LOC code-behind · App ~262 LOC · Spectrogram ~995 LOC.

---

## 3. Per-project findings (condensed)

### SimTuning.Core (domain + calculation + services)
- **ModuleLogic**: `EngineLogic`/`AuslassLogic`/`EinlassLogic`/`DynoLogic`/`AudioLogic`/`TuningLogic` — all `static` except `TuningLogic`.
- **`AuslassLogic.Auspuff(ref VehiclesModel)`** is a **~460-LOC god method** (calc + 2000×1000 layout + 7-segment drawing + labels in one method); triply-nested `if/else` for 1/2/3-stage diffusor duplicated across 4 sections; cases `2`/`3` are stubs. Mutates the ORM entity via `ref` (40+ assignments).
- `EngineLogic.GetSteuerdiagramm` ~120 LOC; `DynoLogic.GetLeistung` takes **11 sequential `double` params** with single-letter names; `GetSteigungskraft` silently rewrites `s=1` when `s==0`.
- **`TuningLogic` is entirely TODO stubs** — dead code.
- **`VehicleService`** (344 LOC): `new DatabaseContext()` per call, **fully synchronous** (`SaveChanges`/`ToList`, no async), ctor calls `EnsureDatabaseCreated()`. Returns `null` from retrieve methods without `?` annotation. Mixed structured/legacy logging templates.
- **`AudioLogic`**: sole consumer of vendored `Spectrogram`; static mutable state (`_epsilon`, `_audioFile`, `SpectrogramAudio`); magic FFT params (`16384`/`25`/`250`/`/1000`).
- **Deprecated crypto**: `RNGCryptoServiceProvider` (`Functions.cs:236`, private/unused), `SecureString` plumbing (`Converts.cs:17,53`).
- **One `async void`**: `Functions.ShowSnackbarDialog` (`Functions.cs:181`).
- **Security**: no raw SQL (all LINQ/EF), no hardcoded secrets. Path-traversal surface in `Functions.CreateZipFile` (source paths unvalidated) and `GeneralSettings.*FilePath` (Preferences-backed, unsanitized). `BrowserService` uses `new HttpClient()` per call and `OpenBrowser(url)` doesn't validate scheme.

### Spectrogram (vendored, ~995 LOC)
- **Only consumer: `Core/ModuleLogic/AudioLogic.cs`.** Used surface: `Spectrogram` ctor + `.Add(double[],bool)` + `.HzPerPx` + `.SecPerPx` + `.GetFFTs()`, and `WavFile.ReadMono(path)`.
- **Dead surface (never called by the app):** `GetBitmap/GetBitmapMax/GetBitmapMel`, `Colormap` enum (viridis/jet/…), `SFF` (.sff reader/writer), `Tools`, `Image.ApplyColormap`, plus 3 `[Obsolete(...,true)]` methods.
- `AllowUnsafeBlocks=true` in `Spectrogram.csproj` but **zero `unsafe` usage** — flag is unused.
- `SFF.Load` parses an untrusted binary file with `BitConverter` and **no bounds checks** on header offsets 40–95 (buffer over-read on malformed input).
- Empty `catch {}` in `SFF.cs:101` swallows `IndexOutOfRange`.
- **Inherits the MAUI multi-TFM** from `Directory.Build.props` — a vendored DSP lib should be `net10.0`-only and not pull iOS workloads.

### SimTuning.Data (EF Core persistence, ~6,200 LOC)
- `net10.0`-only, `UseMaui=false`, `Nullable=disable`. EF Core `10.0.0` (latest stable `10.0.10`; preview `11.0.0-preview.*`). Also pulls `Microsoft.Maui.Controls 10.0.10` via an `Update` ref — because `DatabaseSettings`/`UnitSettings` call `Microsoft.Maui.Storage.Preferences`/`FileSystem` directly (cross-project version coupling).
- `DatabaseContext`: 14 DbSets, `OnConfiguring` → `UseSqlite($"Filename={DatabaseSettings.DatabasePath}")`, no Fluent config at all (conventions + data annotations only — no `HasMaxLength`/`HasIndex`/`IsRequired`/`ValueConverter`).
- **`EnsureDatabaseCreated()` calls `Migrate()` then `EnsureCreated()`** — documented anti-pattern (contradictory).
- `BaseEntityModel`: `[Key] int? Id` (nullable PK smell), `CreatedDate`/`UpdatedDate` stamped in sync `SaveChanges` override — **no async, no concurrency token** (last-write-wins).
- **Massive duplication**: UnitsNet `*Unit` setter pattern copied **~80×** across models; `AuspuffModel.cs` (1,047 LOC) is ~90% this boilerplate.
- 1 migration (`20251113215000_UpdateDatabaseSchema`) + snapshot, consistent. `[Obsolete(true)]` on `DynoNm` DbSet but table still maintained.
- **Security**: no raw SQL (no `FromSqlRaw`/`ExecuteSqlRaw`). **No encryption at rest** (plaintext SQLite; `AusrollenModel` stores lat/long → potential PII). Path-traversal: `DatabaseSettings.FileDirectory`/`DatabasePath` read from `Preferences` and flowed unsanitized into `Path.Combine`/`File.Create`/`Directory.CreateDirectory`.

### SimTuning.Maui.UI (Views + ViewModels, ~7,460 + 542 LOC)
- **`ViewModels/VehiclesViewModel.cs` is `VehiclesViewModelBase` (1,318 LOC)** — a "god base" with **~60 mirrored pass-through properties** (one per `Vehicle.Motor.X.Y` field), each identical get/set boilerplate. Every concrete vehicle VM inherits it. Root cause of the project's LOC bloat.
- Other god VMs: `AuslassAnwendungViewModel` (752), `DynoRuntimeViewModel` (602, **half-commented-out** — location/audio recording flow removed, leaving a timer/stopwatch shell), `AuslassTheorieViewModel` (540), `MotorUmrechnungViewModel` (511), `DynoAudioViewModel` (432).
- **MVVM inconsistency**: `[ObservableProperty]` used **0 times**; `[RelayCommand]` used 9× vs **24 hand-written `new RelayCommand`/`new AsyncRelayCommand`**; only 9/18 VMs are `partial`. `DynoDataViewModel` mixes both styles. `ViewModelBase : ObservableRecipient` never calls `OnActivated` (could be `ObservableObject`).
- **Service-locator anti-pattern**: **22 `Ioc.Default.GetRequiredService` call-sites, all in View code-behind** (not VMs). VMs correctly use constructor injection.
- **Navigation lifecycle** hooks (`OnNavigatedTo`/`From`/`OnNavigatingTo`) **never overridden** → startup loads happen in constructors.
- **Popups**: 4 popups (`VehiclePopup`, `DynoCreationPopup`, `PortTimingPopup`, `EnvironmentPopup`) registered via `AddTransientPopup`. **Likely bug:** `MauiProgram.cs:106-109` maps two popups to the same VM (`VehiclePopup`+`DynoCreationPopup`→`VehiclesViewModel`; `PortTimingPopup`+`EnvironmentPopup`→`PortTimingViewModel`); the second `AddTransientPopup` overwrites the first → **`VehiclePopup` and `PortTimingPopup` are unreachable**. All 10 `ShowPopupAsync` call-sites resolve to the shadowing popup.
- **Async smells**: 6 `async void` (`NavigationService.cs:68,96` + 4 popup handlers); **27 `await` without `ConfigureAwait`**; `System.Timers.Timer` handlers mutate bound properties cross-thread (unsafe); sync IO (`File.WriteAllText`/`Delete`/`ZipFile.ExtractToDirectory`) inside `async Task` methods; `DynoAudioViewModel` exposes sync `IRelayCommand` over heavy CPU work (FFT/clustering/polyfit) on the UI thread.
- **Dead code / magic numbers**: large commented blocks in `DynoRuntimeViewModel` (location/recording) and `DynoDataViewModel.ImportDyno`; magic timer intervals (5000/10000/10/100ms), `GetPowersOf2(13,18)`, etc.
- **Security**: `DynoDataViewModel.ImportDyno` downloads a hardcoded-zip URL → `ZipFile.ExtractToDirectory(FileDirectory)` after deleting pre-existing files, then (in commented code) `JsonConvert.DeserializeObject<DynoModel>` — insecure archive + Newtonsoft-deserialization surface. Switch to per-entry zip validation + `System.Text.Json`.

### SimTuning.Maui.App (host, ~262 LOC)
- `Exe`, `SingleProject`, `UseMaui`, `EnablePreviewFeatures=true`. `SupportedOSPlatformVersion`: iOS **12.2**, MacCatalyst **15.0**, Android **21.0**, Windows **10.0.17763.0**. No csproj `PackageReference`s (all centralized).
- **DI (`MauiProgram.cs`)**: `RegisterServices` all **Singleton** incl. **`DatabaseContext` (Singleton — EF DbContext foot-gun; should be Scoped)**; `IVehicleService`/`IBrowserService`/`INavigationService` Singleton. `RegisterViewModels` all Transient (23 VMs). `RegisterPopups` — see UI bug above.
- **Serilog (`SetupSerilog`)**: `MinimumLevel.Verbose()` with `Microsoft→Warning`, Debug + rolling File (daily, 22 retained) sinks, bridged via `AddSerilog`. **Verbose floor in Release is a security smell.** Log path from `GeneralSettings.LogFilePath` (Preferences-backed, partly user-controlled filename).
- **`App.xaml.cs`**: bridges `Ioc.Default.ConfigureServices(serviceProvider)`; `MainPage` is `new`-ed, not DI-resolved.
- **iOS/MacCatalyst heads**: minimal `AppDelegate`/`Program`. **iOS `Info.plist` missing `NSMicrophoneUsageDescription` + `NSLocationWhenInUseUsageDescription`** — Android manifest requests those perms, but iOS will **crash at runtime** on mic/location and it's an App-Store rejection. No ATS customization (defaults HTTPS-only — fine).
- **Windows head**: only platform with real code — `ToastNotificationManagerCompat.OnActivated` (empty handler); `Package.appxmanifest` declares `runFullTrust` + a COM toast CLSID. `CommunityToolkit.WinUI.Notifications 7.1.2` referenced repo-wide but consumed **only here** → make it a Windows-conditional `ItemGroup`.
- **Tizen**: `Platforms/Tizen/Main.cs` + manifest exist but the `net10.0-tizen` TFM is commented out → dead code.
- **Android**: `allowBackup="true"` (minor smell). `Sharpnado` registered with `debugLogEnable:true` (left on).

### SimTuning.Test (xUnit + Moq)
- `SimTuning.Test.csproj` declares **no `<TargetFramework>`** → inherits mobile TFMs (wrong; see §1). xUnit `2.9.3`, Moq `4.20.72`, `Microsoft.NET.Test.Sdk 18.0.1`, `coverlet.collector 6.0.4`. References **both** `Core` and `Maui.UI`.
- ~42 `[Fact]`. Logic tests (Engine 12, Auslass 5, Einlass 3, Dyno 1, Audio 1) + VM tests (21). `MauiViewModelsTest` mocks `ILogger`/`IVehicleService`/`INavigationService`/`IBrowserService` properly (constructor injection) — no DB hit.
- **Quality problems**: ModuleLogic tests read returns into **unused locals with zero `Assert`** (smoke tests only). **`AudioLogicTest.SpectrogramCreationTest` is missing `[Fact]`/`[Theory]`** → xUnit silently skips it. **`DynoLogicTest.PlotCreationTest` body is fully commented out** (false green). Hardcoded `C:\Users\Tony\AppData\Roaming\SimTuning\…` path (`Constants.cs:15`). Tests write PNGs to `%APPDATA%` → fail on non-Windows/CI. No async tests; every `[Fact]` is `void`. `MauiViewModelsTest` implements an empty `IViewModelTest` contract.

---

## 4. MVVM / DI / logging / data posture

- **MVVM**: CommunityToolkit.Mvvm 8.4.0, `ViewModelBase : ObservableRecipient`. Source generators **under-used** (`[ObservableProperty]` 0×; `[RelayCommand]` 9× vs 24 hand-written). Messenger used correctly for the Dyno cross-VM channel (`CurrentDynoRequestMessage`/`DynoChangedMessage`).
- **DI**: `MauiApp.CreateBuilder()` + `RegisterServices/ViewModels/Popups`. **`DatabaseContext` Singleton** is the standout bug. `Ioc.Default` service-locator bridge used in **22 View code-behinds** — systemic.
- **Logging**: Serilog centralized in `SetupSerilog`, bridged to `Microsoft.Extensions.Logging` via `AddSerilog`. `Verbose` floor in production. Some legacy `{0}` templates in `VehicleService`. No `Console.WriteLine` debugging flagged.
- **Data**: EF Core SQLite, conventions-only config, sync access, plaintext file, `Migrate()+EnsureCreated()`. `DatabaseSettings` couples Data→MAUI via `Preferences`/`FileSystem`.

---

## 5. Analyzer / code-style posture

- **StyleCop.Analyzers 1.1.118** is referenced but **heavily suppressed**: `Code Rules.ruleset` + `.editorconfig` set ~75 SA\* rules to `None` (all documentation rules SA1600–SA1651, plus most layout/ordering/naming rules). `1591` (XML doc) suppressed in `NoWarn`. → Effectively StyleCop enforces very little; **restoring doc rules is a valid long-term goal** (Phase 5+).
- **AsyncFixer 1.6.0** active — will fire on the 6 `async void`, 27 missing `ConfigureAwait`, sync IO in async, unawaited Task.
- **NetAnalyzers**: ruleset includes basic correctness/design/globalization/security; `CA1707` (no underscores) disabled. Several CA rules disabled (`CA1303`, `CA1305` globalization, etc.).
- **Formatting**: `.editorconfig` = tabs/4, UTF-8, trim trailing ws. `.csharpierrc.json` (tabs/4/120/single-quote/auto-EOL). Prettier (`@prettier/plugin-xml`) for XAML. `cspell.json` (en, de-de dictionary words). `.prettierrc.json` present. **Two `.editorconfig` exist** (repo root) — verify no conflict.
- **Nullable**: enabled globally (except `SimTuning.Data`). In practice NRT annotations are sparse in Core/App; better in Maui.UI; absent in Spectrogram (will produce CS8602 flood).

---

## 6. Consolidated security findings (OWASP)

| # | Finding | Location | Phase |
|---|---|---|---|
| A03 | No SQL injection — all EF LINQ, no raw SQL | Data, Core VehicleService | — (clean) |
| A01 | Path traversal via Preferences-backed paths (`DatabasePath`, `*FilePath`, `LogFile`) — unsanitized `Path.Combine` | `DatabaseSettings`, `GeneralSettings` | 11/14 |
| A01 | Unvalidated zip extraction (`ExtractToDirectory`) of downloaded archive into app dir; pre-deletes target files | `DynoDataViewModel.ImportDyno` | 14 |
| A08 | Newtonsoft.Json deserialize of (future) downloaded JSON; switch to System.Text.Json | `DynoDataViewModel.ImportDyno` (commented) | 14 |
| A02 | No encryption at rest (plaintext SQLite); lat/long stored | `DatabaseContext`, `AusrollenModel` | 14 |
| A02 | Deprecated crypto primitives (`RNGCryptoServiceProvider`, `SecureString`) | `Functions.cs`, `Converts.cs` | 14 |
| A05 | `MinimumLevel.Verbose()` in production logging; potential PII in logs | `MauiProgram.SetupSerilog` | 9 |
| A05 | `android:allowBackup="true"`; Sharpnado `debugLogEnable:true` | Android manifest, MauiProgram | 9/16 |
| A06 | `AllowUnsafeBlocks=true` set but unused; SFF untrusted-binary parse w/o bounds checks | `Spectrogram.csproj`, `SFF.cs` | 14/17 |
| — | iOS missing mic/location usage strings (functional + store rejection) | `Platforms/iOS/Info.plist` | 2 (bug) |
| — | SSRF/intent-hijack: `OpenBrowser(url)` no scheme validation; `new HttpClient()` per call | `BrowserService` | 14 |

No hardcoded secrets/credentials/connection strings found anywhere.

---

## 7. Behavioral bugs found (fix regardless of modernization)

1. **iOS mic/location crash** — `Info.plist` missing `NSMicrophoneUsageDescription`/`NSLocationWhenInUseUsageDescription` (Android manifest has the perms). → Phase 2.
2. **Duplicate popup registration** — `VehiclePopup` & `PortTimingPopup` shadowed by `DynoCreationPopup`/`EnvironmentPopup` (same VM); the first pair is unreachable. → Phase 6/8.
3. **`DatabaseContext` Singleton** — captive-context bug; change to Scoped. → Phase 8.
4. **`Migrate()` + `EnsureCreated()` both called** — pick one. → Phase 8/12.
5. **`AudioLogicTest` missing `[Fact]`** (silently skipped); **`DynoLogicTest` body commented** (false green); **hardcoded `C:\Users\Tony\…`** path. → Phase 15.
6. **NAudio Windows-only** — audio conversion path can't run on iOS/Mac/Android. → Phase 3/13 (needs a cross-platform plan).
7. **`throw new Exception();`** with no message in `NavigationService.cs:25`. → Phase 11.

---

## 8. .NET 11 migration risk register (prioritized)

| Pri | Risk | Where | Validation |
|---|---|---|---|
| 🔴 P0 | **No MAUI workloads** — build fails today | env | `sudo dotnet workload restore` (Phase 2 prereq) |
| 🔴 P0 | **NAudio 2.2.1 Windows-only** | `Core/Helpers/AudioUtils` | runtime test on iOS/Mac; plan replacement |
| 🔴 P0 | **Mono→CoreCLR** switch (.NET 11 Preview 4+) affects native interop/GC | NAudio capture, SkiaSharp render, FftSharp/Spectrogram DSP, DBSCAN | real device/sim validation, not just desktop |
| 🟠 P1 | **`Microsoft.Maui.Controls` 10.0.10 → 11.x preview** | all TFMs | coordinated workload + package bump (Phase 2/3) |
| 🟠 P1 | **EF Core 10 → 11 preview** | Data | re-scaffold migration, diff vs `20251113215000`; watch 1—1 convention + nullable-PK behavior |
| 🟠 P1 | **LiveChartsCore `2.0.0-rc4.5`** (pre-release) + **SkiaSharp `3.119.1`** | Dyno plot VMs, MotorSteuerdiagramm | confirm net11-compatible build; retest `Alpha8`/`CreateTable`/SKColors field init |
| 🟠 P1 | **CommunityToolkit.Maui.MediaElement 6.1.3** | `DynoAudioView` (6 event handlers) | highest iOS/Mac breakage rate across MAUI versions — explicit retest |
| 🟡 P2 | **Sharpnado TaskLoaderView 2.6.3 / Tabs 4.0.1** | MauiProgram | historically trails MAUI; check net11 availability |
| 🟡 P2 | **`System.Timers.Timer` cross-thread** property changes | `DynoRuntimeViewModel` | migrate to `IDispatcherTimer` |
| 🟡 P2 | **`async void`** (6 sites) | NavigationService + 4 popups | wrap in try/catch + log |
| 🟡 P2 | **Newtonsoft.Json 13.0.4** import path | DynoDataViewModel | migrate to System.Text.Json |
| 🟢 P3 | `CommunityToolkit.WinUI.Notifications 7.1.2` old TCM | Windows head | Windows-conditional ref + retest COM activator |
| 🟢 P3 | **Data→MAUI coupling** via `DatabaseSettings` (Preferences/FileSystem) | Data | decouple with injected interface |
| 🟢 P3 | **Spectrogram inherits MAUI TFM** | Spectrogram.csproj | make `net10.0`-only + `Nullable=disable` |

---

## 9. Phase 1 → roadmap TODO (seeds Phases 2–18)

- **Phase 2** — workload restore (sudo), `global.json` pin, TFM → `net11.0-*` (+ Data override + **Test pin to `net11.0`-only**), iOS Info.plist usage strings, platform version review.
- **Phase 3** — bump Maui.Controls/EF Core to preview; validate LiveCharts/SkiaSharp/MediaElement/Sharpnado/FftSharp/DBSCAN net11 builds; plan NAudio cross-platform replacement; make `WinUI.Notifications` Windows-conditional.
- **Phase 4** — fix the ~75 suppressed-but-real analyzer hits (async void, missing ConfigureAwait, sync IO, deprecated crypto) properly, not via `NoWarn`.
- **Phase 5** — file-scoped namespaces, `[ObservableProperty]`, `record`/`init`/`required`, pattern matching; defer Data nullable until dedicated review.
- **Phase 6** — break up `VehiclesViewModelBase` (source-gen the mirrors), `AuslassLogic.Auspuff`, god VMs; fix popup-registration bug; standardize on `[RelayCommand]`/`[ObservableProperty]`.
- **Phase 7** — async/await + `CancellationToken` + `IDispatcherTimer`; remove `async void`/sync IO in async.
- **Phase 8** — `DatabaseContext` Scoped; constructor injection in Views (remove 22 `Ioc.Default` calls); resolve popup dup; decouple Data from MAUI Storage.
- **Phase 9** — Serilog levels per environment; structured logging; remove `Verbose` in Release.
- **Phase 11** — replace empty catches; typed exceptions (`NavigationService`).
- **Phase 12** — DRY the UnitsNet `*Unit` boilerplate (value converter); dedup `VehiclesViewModelBase` mirrors.
- **Phase 14** — zip-entry validation, System.Text.Json, path canonicalization, encrypt-at-rest decision, remove deprecated crypto.
- **Phase 15** — real assertions; fix dead tests; cross-platform test paths; async tests.
- **Phase 16** — delete Tizen, dead Spectrogram surface (until Phase 17), `TuningLogic` stubs, commented blocks.
- **Phase 17** — replace vendored Spectrogram with NuGet (confirm `AudioLogic` is sole consumer — done; consumed surface documented in §3).

---

## 10. Verification status (this phase)

- **Build: FAILS** — `NETSDK1147`, missing `maui-ios` workload (5 of 6 projects; only `SimTuning.Data` builds). **No compiler/analyzer warnings captured** (build stops at workload resolution). Warning capture is deferred until workloads are restored (Phase 2 prereq).
- **Tests: not run** — same workload blocker, plus the test project's wrong TFM.
- **`dotnet` CLI**: at `/usr/local/share/dotnet/dotnet`, **not on PATH** for this shell.
- **Graphify graph**: current enough for structure (`e0ecdbae` includes the popup work); 3 newer commits are non-structural.
