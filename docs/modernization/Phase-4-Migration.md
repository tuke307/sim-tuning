# Phase 4 — Fix Compilation (IN PROGRESS)

> Branch `modernize/phase2-net11` (off `v2.0.0`). SDK `11.0.100-preview.6`.
> Goal: resolve compiler/analyzer warnings **properly** — no new `<NoWarn>`, no blanket `!`/`#pragma`.
> Resumable from this doc alone. Last updated mid-phase.

## How to resume

1. `export PATH="/usr/local/share/dotnet:/Users/MEISSTO/.dotnet/tools:$PATH"` (`dotnet` is **not** on PATH; csharpier is a global tool at `~/./.dotnet/tools`).
2. Read **§ Gotchas** below before touching anything — several footguns (formatter tabs, phantom prettier).
3. Finish the **§ Pending** list. Single source of truth = `dotnet build` + the dedup script in §Baseline.

## Baseline (start of Phase 4)

Full-solution build had **484 unique warnings** (1438 raw, duplicated per-TFM). Dedup script:

```bash
grep -E ": warning " <buildlog> | sed -E 's/\[\/.*$//' \
  | sed -E 's|/Debug/(net11\.0[^/]*)/|/TFM/|g' | sort -u
```

Top codes: CS0618×196, CS8603×67, CS8618×61, CS8605×42, CS8629×23, CS0612×21, CS8625×14, SA1027×12, CS8767×12, CS8622×10, CS8600×5, CS8601×4, MAUIG2045×3, CS8604×3, CS0169×2, AsyncFixer02×2, xUnit1008×1, SYSLIB0023×1, SA1128×1, CS8981×1, CS8619×1, CS8602×1, CS0414×1.

After Phase-2 commit the README TODO quoted raw counts (CS8618 ×300 …) — those were per-TFM; the deduped reality is ~¼ of that.

## ✅ DONE — committed

### Commit `72d8447` — obsolete APIs + formatter setup
- **SkiaSharp 3.x SKFont migration** (CS0618): `SKPaint.TextSize` + `DrawText(string,x,y,SKPaint)` are obsolete. `AuslassLogic.cs` (×19) and `EngineLogic.cs` (×3): dropped `TextSize` from the paint, introduced one `using SKFont labelFont = new SKFont { Size = N }`, switched to `DrawText(s,x,y,SKTextAlign.Left,labelFont,blackPen)`. Rendering unchanged (default typeface, same sizes).
- **`Functions.cs`**: removed dead private `Generate128BitsOfRandomEntropy()` (sole `RNGCryptoServiceProvider` use → SYSLIB0023; Phase 1 flagged it unused) + orphaned `using System.Security.Cryptography;`. Also fixed CS8600 (`string? messageKey`) and CS8604 (`content.ToString() ?? string.Empty`).
- **`NavigationService.cs`**: `Application.MainPage` (deprecated MAUI 11) → `Application.Current?.Windows.FirstOrDefault()?.Page?.Navigation`.
- **`.csharpierrc.json`**: `useTabs: true → false` (see §Gotchas — this was a real config bug).
- **`.csharpierignore`**: excludes generated/vendored C# (MaterialDesignIcons.cs, *.Designer.cs, obj/bin).
- **`Spectrogram.csproj`**: `<Nullable>disable</Nullable>` on vendored DSP (Phase 1 recommendation; replaced in Phase 17).

### Commit `3e42c01` — XAML obsolete (source-generated)
- **`*AndExpand` layout options** (CS0618 ×173): `CenterAndExpand→Center`, `StartAndExpand→Start`, `FillAndExpand→Fill` across 17 XAML (168 tokens). Standard MAUI migration; removes the "expand" claim on StackLayout children → layouts with extra space may shift slightly. **Flagged for device visual review.**
- **`FontSize="<NamedSize>"`** (CS0612 ×21): the string shorthand routes through deprecated `Device.GetNamedSize` (which returns platform-**dynamic** values — the reason it's deprecated). Replaced with explicit doubles on the app's own `Global.xaml` scale: Medium=14, Subtitle=16, Caption=12 (app defines FontSizeSmall=10/Medium=14/Large=18).

## ✅ DONE — uncommitted (Core verified; VMs subagent-produced, NOT yet build-verified)

### Core + Behaviors + Styles (done by orchestrator; **Core builds clean: 0 err, 0 CS86xx, 22 warnings**)
- `Converts.cs`: `StringToSecureString` → returns `SecureString?`; `Marshal.PtrToStringUni(...) ?? string.Empty`.
- `AudioUtlis.cs`: `TrimWavFile` params `string? inPath=null, string? outPath=null, Stream? inStream=null`.
- `AudioLogic.cs`: `_audioFile?`; `RotationalSpeedPoints`/`ClusterPoints` `= new()`; `SpectrogramAudio?`; `HzPerFFT`/`SegmentsPerSecond` `SpectrogramAudio?.X ?? 0`; `SpecData` getter `?? new List<double[]>()`.
- `DynoLogic.cs`: `(double)observablePoint.X` → `observablePoint.X ?? 0` (×2, for DynoPsModel + DrehzahlModel).
- `AuslassLogic.cs`: all `.Value` → `.GetValueOrDefault()` (9 double? sites); `(int)EndrohrL/EndrohrD` → `(int)(… ?? 0)` (only EndrohrL/EndrohrD warned — they're the only model fields never assigned in the calc, so the analyzer knows they may be null).
- `VehicleService.cs` + `IVehicleService.cs`: 8 retrieve/create method **returns → nullable** (`DynoModel?`, `VehiclesModel?`, `List<…>?`). Honest: they already returned null on error/not-found. ⚠️ **Cascades to callers** (see §Pending).
- `BehaviorBase.cs`: `AssociatedObject` → `T?`; `OnBindingContextChanged(object? sender, …)`; `BindingContext = AssociatedObject?.BindingContext`.
- `EventToCommandBehavior.cs`: `Delegate? eventHandler`; `object? resolvedParameter`; added `AssociatedObject is null` guards to `DeregisterEvent`/`RegisterEvent` + `MethodInfo?` null guard (no `!` needed).
- `Styles/{Colors,Global,Icons}.xaml.cs`: csharpier'd (single-file) to clear SA1027.

### ViewModels (subagents — files edited, NOT build-verified)
- **Motor + Einlass** (DONE): MotorUmrechnung, MotorVerdichtung, MotorHubraum, MotorSteuerdiagramm, EinlassKanal, EinlassVergaser. Pattern: nullable backing fields/props; `(LengthUnit?)value?.UnitEnumValue` casts; `.Value`→`.GetValueOrDefault()`. ⚠️ Agent flagged possible **new CS8602 at `.UnitEnumValue` derefs** (left for build).
- **Auslass** (DONE): AuslassTheorie, AuslassAnwendung. Added `|| value == null` to setter guards; `AuslassAnwendung.Calculate` uses `if (Vehicle is not VehiclesModel vehicle) return;` to keep `ref VehiclesModel` non-null (no `!`). `_helperVehicle` made nullable (not deleted — orchestrator removes it, see §Pending).
- **Vehicles** (DONE): VehiclesViewModelBase (god-class) + Vehicles/VehiclesViewModel. `_engine`/`_vehicle` nullable; `_engines`/`_vehicles` `= new()` (deviation — avoids cascade); 26 getters nullable; 19 nullable-target + 3 non-nullable-target casts. ⚠️ Agent flagged possible **CS8602 on `?.`-chain setter narrowing (~30 sites)** — Roslyn flow analysis through `if (Vehicle?.X?.Y == null) return;` then `Vehicle.X.Y.Z = value`. Fix if flagged: capture chain into a local.
- **Dyno + small** (⏳ STILL RUNNING — subagent not yet finished): DynoAudio, DynoData, DynoDiagnosis, DynoMain, DynoAusrollen, DynoGeschwindigkeit, DynoRuntime, Environment, PortTiming.

## ⏳ PENDING (resume here)

1. **Wait for Dyno subagent** to finish (9 files).
2. **Central build + dedup**: `dotnet build SimTuning.sln` (tolerate the iOS env error), run the dedup script, compare to baseline (484).
3. **Mop up cascades** the central build surfaces:
   - VehicleService nullable returns → **caller sites** need `?? new()` / `?? Enumerable.Empty()`. Known: `VehiclesViewModelBase.ReloadData` (`new ObservableCollection<>(_vehicleService.RetrieveVehicles())` → CS8604), and similar `RetrieveMotoren()`/`RetrieveDynos()` callers across VMs.
   - Motor agent's `.UnitEnumValue` derefs (CS8602) — guard or capture local.
   - Vehicles agent's `?.`-chain setter narrowing (CS8602) — capture locals.
   - Any other nullable-cascade from the subagent edits.
4. **Dead fields** (clears CS0169 ×2 + CS0414 ×1; also clears the CS8618 on `_locationService`):
   - `AuslassAnwendungViewModel._helperVehicle` (never used).
   - `DynoRuntimeViewModel._locationService` (never used) + `trackingStarted` (assigned, never read).
   - These are remnants of commented-out location/recording flow. Safe to delete.
5. **Remaining SA1027** (MotorUmrechnungViewModel, MotorVerdichtungViewModel) — csharpier single-file once those subagent files are stable.
6. **Verify, format, docs**:
   - `dotnet build SimTuning.sln` green (except iOS env).
   - `dotnet test` (still 0 discovered — `xunit.runner.visualstudio` missing, Phase 15).
   - `graphify update .`
   - Finalize this doc (fill result counts), update `README.md` index + running TODO.
   - Commit the nullable batch + dead-field removal.
7. **Summarize** breaking changes (AndExpand layout shift, FontSize named→explicit, VehicleService returns now nullable).

## ⛔ Deferred out of Phase 4 (document, don't fix here)

| Item | Count | Phase |
|---|---|---|
| `AsyncFixer02` (sync IO in async: `File.WriteAllText`, `ZipFile.ExtractToDirectory` in DynoDataViewModel) | 2 | 7 |
| `xUnit1008` (test-data attr on non-Theory) | 1 | 15 |
| Test `CS8625` (`vm.NewDyno(null)` etc. — null to non-nullable VM params) | 5 | 15 |
| `ListView` deprecated → CollectionView (in `Styles/Global.xaml` style) | 6 | 6/12 (UI-control migration) |
| `MAUIG2045` binding source-gen hint (`VehicleMotorAuslassAuspuffDiffusorStage`) | 3 | 6 (works via reflection) |

## 🚨 Gotchas (READ BEFORE RESUMING)

1. **`.csharpierrc.json` `useTabs` must stay `false`.** The codebase is **space-indented** and StyleCop SA1027 enforces spaces; `useTabs:true` caused a 362-warning SA1027 flood on any file csharpier touched. (`.editorconfig` says `indent_style = tab` but the real code + SA1027 want spaces — leave .editorconfig alone, it's separate debt.)
2. **NEVER run `csharpier format <directory>` (recursive).** It triggers a **phantom prettier pass** that reformats ~70 XAML/XML/csproj/props files to tabs (huge churn). Use **single-file** `csharpier format <path/to/file.cs>` only — verified safe. (Cause never fully pinned; likely a hook on the recursive invocation. Idle-watcher test confirmed it's a one-time trigger, not continuous.)
3. **`dotnet` is at `/usr/local/share/dotnet/dotnet`, not on PATH.** Always `export PATH="/usr/local/share/dotnet:/Users/MEISSTO/.dotnet/tools:$PATH"` first.
4. **iOS target build error is environmental & pre-existing** (missing simulator runtime). Ignore it; run `xcodebuild -downloadPlatform iOS` to fix (~5 GB, optional). **MacCatalyst is the green desktop target.**
5. **`global.json` pins `11.0.100-preview.6`** (`allowPrerelease: true`). Only the preview SDK is installed (no .NET 10 side-by-side).
6. Formatters: csharpier 1.3.0 (global tool), prettier + @prettier/plugin-xml (devDeps). **cspell was installed to node_modules but is NOT a committed devDep** (deferred — German UI makes it noisy).
7. No `dotnet build` was run by the VM subagents (instruction, to avoid concurrent-build races). Their edits are unverified — the central build is the first verification.

## Decisions & rationale
- **Why parallelize nullable across subagents**: 224 hand-written CS86xx across ~30 VM files, uniform patterns (nullable backing fields → nullable props → `.GetValueOrDefault()` / `double?` for unboxing → `object? sender`). Disjoint file ownership; orchestrator builds centrally.
- **Why `_engines`/`_vehicles` use `= new()` not `?`** (Vehicles agent): they ARE set in the ctor via `ReloadData()`; `= new()` is more honest than nullable and avoids ~8 cascade CS8602 sites.
- **Why VehicleService returns went nullable (not `return new List()`)**: preserves behavior (null-on-error stays null-on-error, just honestly typed); the alternative (empty list) would be a behavior change. Caller mop-up is the cost.
- **Why AndExpand→base not Grid-restructure**: Grid is the "correct" MAUI fix but invasive/risky across 17 XAML; `XAndExpand→X` is the documented lightweight migration. Visual delta flagged for device review.
