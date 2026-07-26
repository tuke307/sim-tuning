# Phase 6 — Structural modernization (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: break up the two god-classes, fix the
> duplicate-popup wiring, and standardize on `[RelayCommand]`. Scope was chosen
> **Deep** (Phase-12 mirror-DRY pulled forward). All four sub-tasks are
> **behavior-preserving** — public property/command names that XAML binds to are
> kept byte-identical.

## Result

| Sub-task | Outcome |
|---|---|
| Duplicate popup registration | Dead popups (`DynoCreationPopup`, `EnvironmentPopup`) deleted; live pair kept. **Phase-1 doc corrected** (it had the shadow direction backwards). |
| `AuslassLogic.Auspuff` (~460-LOC method) | Split into `Calculate` + `ComputeGeometry` + 4 `Draw*` helpers + an `AuspuffGeometry` record; main method is a short orchestrator. |
| `VehiclesViewModelBase` (1,318-LOC god base) | Split into 6 partial files; ~60 mirror props collapsed via a single `SetMirror` helper; `raiseAllPropertyChanged()` → `OnPropertyChanged(string.Empty)`. |
| Source-generator standardization | 24 hand-written `new RelayCommand/AsyncRelayCommand` across 9 VMs → `[RelayCommand]` methods. |

**Build:** MacCatalyst head builds **green, 0 errors, 0 new warnings**
(warning-neutral vs HEAD — see §Verification). iOS head still environmentally
blocked (Phase-2).

## Applied (this phase)

### 1. Popup registration fix (also a Phase-1 doc correction)

The Phase-1 analysis asserted the *second* `AddTransientPopup` overwrites the
first, making `VehiclePopup`/`PortTimingPopup` unreachable. **Verified against
the CommunityToolkit.Maui source** (via deepwiki): `PopupService` registers the
VM→view map with **`TryAdd` → first registration wins**. So the live popups are
`VehiclePopup` + `PortTimingPopup`; `DynoCreationPopup` + `EnvironmentPopup`
were the **dead** pair. (Acting on the doc as-written would have deleted the
live popups and broken the app.)

- Deleted `Views/Popups/DynoCreationPopup.xaml{,.cs}` and `EnvironmentPopup.xaml{,.cs}`.
- `MauiProgram.RegisterPopups`: removed the two shadow `AddTransientPopup` lines (kept the live pair), with a comment documenting the first-wins rule.
- Corrected `Phase-1-Analysis.md` §0/§3/§7 and `README.md` to reflect first-wins.

### 2. `AuslassLogic.Auspuff` extraction

`src/SimTuning.Core/ModuleLogic/AuslassLogic.cs` — the 460-LOC method had 8
clean `#region`s, each extracted verbatim into a `private static` helper:

- `Calculate(ref VehiclesModel)` ← Berechnung (calc + diffusor switch).
- `ComputeGeometry(VehiclesModel) → AuspuffGeometry` ← Groeßen + Positionshilfen + stage flags (`HasD1/2/3`, computed once — replaces the triply-nested `if/else` duplicated across 3 draw regions).
- `DrawFramework` / `DrawDimensionLines` / `DrawValueLabels` / `DrawPartLabels` ← the 4 draw regions (bodies copied verbatim; each binds the geometry to original-named locals).
- `AuspuffGeometry`: `private readonly record struct` (PascalCase params; `SA1313` suppressed locally as the idiomatic fix for positional records).

The orchestrator `Auspuff(ref vehicle)` is now ~30 LOC. **Behavior-preserving:**
the draw helpers reproduce the exact same `DrawLine/DrawRect/DrawText` calls per
stage; `SKPaint` disposal left as-is (only the font is `using`) — disposal is a
Phase-7/12 concern.

> **CS8629 finding:** the `(int)vehicle.Motor.Auslass.Auspuff.KruemmerL` casts
> (the model fields are `double?`) did **not** warn in HEAD because Berechnung
> (which assigns them from non-null `double` returns) lived in the *same method*,
> so the nullable-flow analysis proved them non-null. Splitting calc and geometry
> into separate methods lost that intra-method analysis → 24 false-positive
> CS8629. Suppressed locally with a documented `#pragma` (the cast expressions
> are byte-identical to HEAD → identical IL; the values are guaranteed non-null
> at runtime since `Auspuff()` always calls `Calculate()` first).

### 3. `VehiclesViewModelBase` split + mirror collapse

`src/SimTuning.Maui.UI/ViewModels/VehiclesViewModel.cs` (1,318 LOC) → 6 partials:
`VehiclesViewModelBase.cs` (core) + `.Motor` / `.Auslass` / `.Einlass` /
`.Ueberstroemer` / `.Dyno`.

- **`SetMirror<TParent>(TParent? parent, Action<TParent> apply, string? raisedName = null)`** — one helper collapses both mirror patterns. Value mirrors: `set => SetMirror(Vehicle?.Motor, m => m.BohrungD = value);` (~10 LOC → 4). Unit mirrors pass `nameof(PairedValue)` as `raisedName`. The unit cast lives in the caller's lambda (only 3 fields need `.GetValueOrDefault()`: `Auslass.{FlaecheAUnit,HoeheHUnit,LaengeLUnit}`; everything else is a nullable cast).
- **`raiseAllPropertyChanged()` → `OnPropertyChanged(string.Empty)`** — a strict superset of the old 70-name list; fixes-by-removal (the duplicate `VehicleMotorHubL`/`VehicleMotorName`/`VehicleDynoAudio` raises and the zero-width `nameof` were all no-ops).
- `[CallerMemberName]` does **not** help for unit mirrors (it yields the unit-prop name, not the paired value).

> **U+FEFF correction:** the Phase-6 plan (and a Plan agent) flagged
> `VehicleMotorDeachsierungL` as carrying a load-bearing U+FEFF. Hexdump proved
> the property declaration is **plain ASCII**; the U+FEFF lived only inside the
> `nameof` in the (now-deleted) `raiseAllPropertyChanged`. C# treats U+FEFF as an
> ignorable Cf formatting char, so the XAML binding (plain ASCII) already worked
> — there was **no latent binding bug**. All 60 mirrors are normal; the U+FEFF
> simply vanishes with the old method.

**Public-surface invariant:** `diff` of every `public` member name before vs
after the split is **empty** (69 members, byte-identical) — no XAML binding can
break. The commented-out `VehicleFrontA`/`VehicleFrontAUnit` dead code is
preserved verbatim in the core partial.

### 4. `[RelayCommand]` standardization

24 hand-written command fields across 9 VMs → `[RelayCommand]` methods; every
converted VM made `partial`. Two sub-patterns: named-method (`[RelayCommand]` on
the existing method) and lambda (extracted to a private `[RelayCommand]` method).
The one new warning introduced (`AsyncFixer01` on the extracted `ShowAusrollenAsync`)
was fixed by returning the `Task` directly.

**Name-mapping trap:** `[RelayCommand]` derives the command name from the method
name (strips trailing `Async`); there is no override. Two async commands had
method names that wouldn't yield the XAML-bound name, so the methods were
**renamed** (no internal callers, verified): `StartBeschleunigung`→`StartAcceleration`,
`ResetRun`→`ResetAcceleration`.

**Disabled commands left as nullable fields** (converting them would re-enable
disabled features — a behavior change): `ShowDiagnosisCommand`,
`ShowSpectrogramCommand`, `StopAccelerationCommand`, `RefreshAudioFileCommand`.

**Invariant:** all 26 XAML-bound `*Command` names still resolve (0 missing) —
either as generated `[RelayCommand]` properties or the intentional nullable fields above.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| `SKPaint` disposal in `AuslassLogic` | Left as-is to stay behavior-identical; resource cleanup is separate. | 7/12 |
| 2/3-stage diffusor *calculation* stubs (`case 2/3` zero everything) | Incomplete feature logic, not a refactor target. | feature work |
| `[ObservableProperty]` beyond commands | Mirrors delegate to the model (no backing field) → doesn't apply; revisit after Phase-12 value-converter. | 12 |
| AuslassLogic CS8629 "real" fix (propagate non-null across methods, or `?? 0`) | Suppressed locally for now; a typed-geometry or non-null-model path would remove the pragma. | 12 |
| `DynoDataViewModel` AsyncFixer02 (sync IO in async), `DynoRuntimeViewModel` dead fields | Pre-existing, documented in Phase-4. | 7 / 16 |
| `Styles_Global` ListView→CollectionView CS0618 | Pre-existing (Phase-4 deferred). | 6/12 |

## Verification

- **Builds (dotnet at `/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Core` — 0 errors.
  - `SimTuning.Maui.App -f net11.0-maccatalyst` — **0 errors, 0 new warnings.**
  - iOS head environmentally blocked (Phase-2), ignored.
- **Warning-neutrality** (authoritative): hard-cleaned HEAD Core = 0 CS8629; Phase-6 Core (with the localized pragma) = 0 CS8629. The MacCatalyst warning *rule set* is identical to HEAD (`Styles_Global` CS0618, `DynoData` AsyncFixer02, `resources` CS8981, `DynoRuntime` CS0414/CS0169 — all pre-existing).
- **Public-surface diff** (`VehiclesViewModelBase`): before-vs-after `public` member names — empty diff (69 members).
- **Command-name invariant:** 26 XAML-bound `*Command` names → 0 missing.
- **No `DynoCreationPopup`/`EnvironmentPopup` references** remain (grep clean).
- **No automated VM tests** (`MauiViewModelsTest` mocks services only; `dotnet test` is 0-discovered, adapter missing — Phase 15). Confidence rests on the behavior-preserving nature of each refactor + the static invariants above. A device smoke test (load vehicle → swap → edit value → change unit) is recommended but not run here.

## Carry-forward

- Device visual/behavior smoke test of the vehicle swap + unit-change flows (exercises `SetMirror` + `OnPropertyChanged(string.Empty)`).
- Remove the AuslassLogic CS8629 pragma via a non-null model path or typed geometry (Phase 12).
- `[ObservableProperty]` sweep after Phase-12 value-converter work.
