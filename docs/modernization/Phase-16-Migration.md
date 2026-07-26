# Phase 16 — Dead-code deletion (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: delete the dead platform target (Tizen), the
> all-stub `TuningLogic`, and the large commented-out code blocks (Phase-1 §9/§16). The
> vendored Spectrogram dead surface was already removed in Phase 17.

## Result

| Sub-task | Outcome |
|---|---|
| Tizen platform | `Platforms/Tizen/` deleted (Main.cs + manifest). The `net*-tizen` TFM was commented out — never built. |
| `TuningLogic` | Deleted (3 all-TODO-stub methods, 0 references in prod or tests). |
| Commented-out code blocks | Removed the named dead blocks in `DynoDataViewModel.ImportDyno` and `DynoRuntimeViewModel` (`OnLocationUpdated`, `StartRecording`). |

**Build:** MacCatalyst head **green, 0 errors, 0 new warnings** (12, unchanged); Test **0 errors**.
**~150 LOC of dead code removed** (99 from the two ViewModels + TuningLogic + the Tizen files).

## Applied (this phase)

### 1. Tizen platform removed
`src/SimTuning.Maui.App/Platforms/Tizen/Main.cs` + `tizen-manifest.xml` deleted. The TFM
(`net11.0-tizen`) was commented out in both `SimTuning.Maui.App.csproj` and
`Directory.Build.props`, so the platform was never built — pure dead code.

### 2. `TuningLogic` deleted
`src/SimTuning.Core/ModuleLogic/TuningLogic.cs` — all three methods (`DefinePlot`,
`OriginalSeries`, `TuningSeries`) were TODO stubs; **0 references** in production code or
tests (verified). Deleted entirely.

### 3. Commented-out code blocks

- **`DynoDataViewModel.ImportDyno`** — removed the trailing dead blocks: the
  `/* OpenAppPackageFileAsync … */` block, the stale `// wenn Datei ausgewählt …` /
  `// if (status) …` lines, and the `// TODO: only for testing /* … Deserialize … */` block.
  The method now ends cleanly after the Zip-Slip-validated extraction (Phase 14).
- **`DynoRuntimeViewModel`** — removed two fully-commented, unreferenced methods:
  - `OnLocationUpdated` (the location/recording flow — ~53 lines of commented body, 0 refs).
  - `StartRecording` (the `AudioRecorderService` setup — commented, only ever called from
    other commented-out lines, 0 refs).

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| Broader commented-code sweep across VMs/Logic | Scattered, mostly cosmetic dead comments (the high-value blocks are done). The `//`-line counts in several files are dominated by XML doc comments, not dead code — needs careful per-file review. | follow-up |
| `IViewModelTest` empty contract | Test infrastructure → Phase 15. | 15 |
| 2 dangling `//await StartRecording()` call comments in `DynoRuntimeViewModel` | Harmless dead comments referencing the now-deleted method; cosmetic. | follow-up |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline).
  - `SimTuning.Test` — **0 errors** (TuningLogic/Tizen removal didn't break test compilation).
  - iOS head environmentally blocked (Phase-2).
- **`TuningLogic`** — 0 references remain (grep clean).
- **Tizen** — TFM stays commented out; no live reference to the deleted platform code.
- **Commented blocks** — `ImportDyno` ends cleanly; `OnLocationUpdated`/`StartRecording` gone (only 2 harmless dangling `//await StartRecording()` call-site comments remain).
- Warning baseline unchanged (12).

## Carry-forward

- Broader commented-code sweep (per-file, cosmetic).
- `IViewModelTest` empty contract → Phase 15.
