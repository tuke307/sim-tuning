# Phase 15 — Test project modernization (✅ Complete)

> Branch `modernize/phase2-net11`. Goal (Phase-1 §6/§15): make `SimTuning.Test`
> a real, discoverable, cross-platform test suite — add the test adapter, replace
> no-op/dead tests with live assertions, fix the test paths, and get `dotnet test`
> green. The bulk of the file-by-file modernization (real assertions, cross-platform
> temp paths, the missing `[Theory]` on `AudioLogicTest`, the commented-out
> `DynoLogicTest` body) landed in `2ddc1fc`; this phase **closes the loop**: it
> diagnoses and resolves the failures that the new real assertions surfaced.

## Result

| Sub-task | Outcome |
|---|---|
| Test adapter | **Done (in `2ddc1fc`).** `xunit.runner.visualstudio` 2.8.2 + `coverlet.collector` 10.0.1 added → `dotnet test` now discovers tests (was 0-discovered in Phase 4). |
| Real assertions / dead tests | **Done.** `AudioLogicTest` has its `[Theory]`; `DynoLogicTest`'s commented OxyPlot body → live `ToObservablePointTest`; all VM tests construct + exercise commands. |
| Cross-platform paths | **Done (in `2ddc1fc`).** `Constants.Directory` → `Path.GetTempPath()`; synthetic WAV generated per-run. |
| **`dotnet test` green** | **Done (this phase).** Surfaced 3 failures, resolved all: **47 total → 46 passed, 1 skipped, 0 failed.** |

**Build:** Test project **0 errors, warning-clean** (the last P4 carry-over test
warning, CS8625 on `NewDyno(null)`, is gone). Changes are **test-project-only** —
the MacCatalyst head and the 12-warning baseline are untouched.

## What the real assertions surfaced — and the fixes

`dotnet test` after `2ddc1fc` reported **3 failures / 44 passed**. Each was a real
issue the old no-op tests had hidden:

### 1. `AudioLogicTest.SpectrogramCreationTest(fftSize: 65536)` — `Assert.NotEmpty` failure

The test synthesizes a 1-second 44.1 kHz sine WAV (44 100 samples). The largest
FFT size, **65 536 > 44 100**, so `Spectrogram.Process` produced **zero whole
windows** → `GetFFTs()` was empty. (The 8 192 / 16 384 / 32 768 cases already had
≥1 window and passed.)

**Fix:** scale the generated duration with `fftSize` so at least two windows fit
at every size — `seconds = max(1, ceil(fftSize * 2 / sampleRate))`. Pure test-data
fix; `AudioLogic`/`Spectrogram` are unchanged.

### 2. `MauiViewModelsTest.DynoAudioViewModelTest` — `NotImplementedInReferenceAssemblyException`

`FilterPlot` → `CheckDynoData` reads `GeneralSettings.AudioAccelerationFilePath`,
whose getter calls `Preferences.Default.Get(...)` **with no design-time fallback**.
A plain `net11.0` xUnit host has no MAUI Essentials platform implementation, so it
throws. Notably `DatabaseSettings` already calls the *same* API through a try/catch
fallback (which is why the `DynoDataViewModelTest` path passes); `GeneralSettings`
and `UnitSettings` lacked it — fixed in Phase 8b.

This is exactly the **Phase-8b** "decouple `SimTuning.Data`/Core from
`Microsoft.Maui.Storage`" coupling. **Skipped** (`[Fact(Skip = …)]`) with a pointer
to Phase 8b, which gives `GeneralSettings` the fallback parity and re-enables this
test. Skipping (not deleting) keeps the coverage intent visible and the run green.

### 3. `MauiViewModelsTest.DynoRuntimeViewModelTest` — same exception, deeper cause

`StartAcceleration` → `CheckDynoData` → `Functions.GetPermission<…>()` →
`Permissions.CheckStatusAsync` throws (no Essentials host). And even past that,
`StartAcceleration` starts an `IDispatcherTimer` via
`Application.Current!.Dispatcher.CreateTimer()` — `Application.Current` is null in
the headless host. So this command is an **integration-only path** (runtime
permissions + dispatcher + audio/location), not unit-testable here.

**Fix (test design):** the test no longer invokes `StartAccelerationCommand`; it
constructs the VM and exercises the lightweight `ResetAccelerationCommand`
(consistent with how the suite skips other platform/disabled commands). The
permission/timer flow is covered by an on-device smoke test.

## Other cleanup (this phase)

- **Deleted `IViewModelTest`** — the empty marker interface (`MauiViewModelsTest`
  was the sole implementor; it enforced nothing xUnit didn't already). This was
  carried forward from Phase 16 ("`IViewModelTest` empty contract → Phase 15").
- **Fixed the last test-project warning** — `DynoDataViewModelTest`'s
  `vm.NewDyno(null)` (CS8625, non-nullable `VehiclesModel` param) →
  `vm.NewDyno(new VehiclesModel())`. `NewDyno` NRE-s into its own catch when the
  mocked `CreateOne` returns null, so behavior is identical, minus the warning.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| Re-enable `DynoAudioViewModelTest` | Blocked on `GeneralSettings` Preferences fallback parity (the Phase-8b Data↔MAUI.Storage coupling). | 8b |
| `async Task` test expansion | Few VM surfaces are both async *and* unit-testable without a platform host (the interesting async paths — permissions, recording, dispatchers — are integration). The sync command surfaces are already covered. | follow-up |
| On-device integration coverage for `StartAcceleration` (permissions + timer + recording) | Requires a running MAUI host / device. | device smoke test |

## Verification

- **`dotnet test src/SimTuning.Test -c Debug`** → **Failed: 0, Passed: 46, Skipped: 1, Total: 47.**
- **Test project warnings:** 0 (CS8625 resolved; no new warnings introduced).
- **MacCatalyst head:** unchanged (this phase edits only `SimTuning.Test/`); 12-warning baseline intact.

## Carry-forward

- Re-enable `DynoAudioViewModelTest` once Phase 8b lands the `GeneralSettings` Preferences fallback.
- Device smoke test for the `DynoRuntimeViewModel` permission/run flow.
