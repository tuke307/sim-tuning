# Phase 16 — Dead-code deletion (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: delete the dead platform target (Tizen), the
> all-stub `TuningLogic`, and the large commented-out code blocks (Phase-1 §9/§16). The
> vendored Spectrogram dead surface was already removed in Phase 17.
>
> **Completion pass** (this follow-up) closed the two carry-forward items from the original
> pass — the dangling `//await StartRecording()` comments — plus the **"dead fields ×2" Phase 4
> had deferred to P16** (`_locationService`, `trackingStarted`), which became fully dead when the
> original pass deleted their consumers. See [§ Completion pass](#completion-pass-follow-up).

## Result

| Sub-task | Outcome |
|---|---|
| Tizen platform | `Platforms/Tizen/` deleted (Main.cs + manifest). The `net*-tizen` TFM was commented out — never built. |
| `TuningLogic` | Deleted (3 all-TODO-stub methods, 0 references in prod or tests). |
| Commented-out code blocks | Removed the named dead blocks in `DynoDataViewModel.ImportDyno` and `DynoRuntimeViewModel` (`OnLocationUpdated`, `StartRecording`). |

**Build:** MacCatalyst head **green, 0 errors**; warning baseline **12 → 10** after the
completion pass removed the 2 dead-field warnings (CS0169/CS0414). Test **0 errors**.
**~150 LOC of dead code removed** in the original pass (99 from the two ViewModels +
TuningLogic + the Tizen files); the completion pass removed a further ~12 LOC (2 fields,
4 now-dead assignments, and the dangling comments).

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

## Completion pass (follow-up)

The original pass left two cosmetic carry-forwards and inherited the Phase-4 "dead fields ×2"
deferral. Both are now resolved:

### 4. Dangling `//await StartRecording()` comments removed
The two `//await StartRecording().ConfigureAwait(true);` lines in `DynoRuntimeViewModel`
(one in `StartAusrollen`, one in `StartAcceleration`) referenced the method deleted in §3.
Removed along with their now-orphaned blank lines.

### 5. Dead fields `_locationService` + `trackingStarted` removed (Phase-4 → P16 deferral)
Phase 4 deferred two dead-field warnings to P16. The original P16 pass deleted the fields'
*consumers* (`OnLocationUpdated` consumed `_locationService`; `StartRecording`/the run flow
read `trackingStarted`) but not the fields themselves, leaving them fully dead:

- **`_locationService`** (CS0169, never used) — declaration only; its constructor parameter
  was already commented out. Deleted the declaration.
- **`trackingStarted`** (CS0414, assigned-but-never-read) — 4 write-only assignments across
  `EndRunAsync`/`OnCountdownTimedEvent`/the stopwatch-stop path/`StartAusrollen`, 0 reads.
  Deleted the declaration, the 4 assignments, and the immediately-orphaned dead comments
  (`// Stop tracking`, `// tracking und recording stoppen`, `//await recorder.StopRecording();`).

Both removals are behavior-preserving by the compiler's own dead-code analysis. The constructor
signature is unchanged (the `_locationService` param was already commented), so
`DynoRuntimeViewModelTest` is unaffected.

**`IViewModelTest` empty contract** — resolved in Phase 15 (the marker interface was deleted
there; see [Phase-15-Migration.md](./Phase-15-Migration.md)).

## Deferred (with rationale)

| Item | Reason | Status |
|---|---|---|
| Broader commented-code sweep across VMs/Logic | Scattered, mostly cosmetic dead comments (the high-value blocks are done). The `//`-line counts in several files are dominated by XML doc comments, not dead code — needs careful per-file review. | follow-up |
| ~~`IViewModelTest` empty contract~~ | Test infrastructure. | **✅ resolved in Phase 15** |
| ~~2 dangling `//await StartRecording()` comments~~ | Referenced the deleted method. | **✅ resolved (this pass)** |
| ~~Dead fields `_locationService` / `trackingStarted`~~ | Phase-4 "dead fields ×2" deferral; orphaned by §3. | **✅ resolved (this pass)** |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors**; warning baseline **12 → 10** (the 2 dead-field warnings CS0169/CS0414 gone; remaining are the pre-existing deferred set — CS0618 ListView ×6, MAUIG2045 ×3, CS8981 ×1).
  - `SimTuning.Maui.UI -f net11.0` — **0 errors**, same warning reduction (shared source file).
  - `SimTuning.Test` — **0 errors**.
  - iOS head environmentally blocked (Phase-2).
- **`TuningLogic`** — 0 references remain (grep clean).
- **Tizen** — TFM stays commented out; no live reference to the deleted platform code.
- **Commented blocks** — `ImportDyno` ends cleanly; `OnLocationUpdated`/`StartRecording` gone; the 2 dangling call-site comments are now gone too.
- **Dead fields** — 0 residual references to `_locationService`/`trackingStarted` (grep clean).

## Carry-forward

- Broader commented-code sweep (per-file, cosmetic) — e.g. the commented `//ILocationService locationService,` constructor param and `//private AudioRecorderService recorder;` in `DynoRuntimeViewModel`, which are silent (no warnings) and part of the scattered sweep.
