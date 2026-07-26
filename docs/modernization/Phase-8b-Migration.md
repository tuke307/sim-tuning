# Phase 8b — Settings-layer decoupling from MAUI.Storage (🔶 Partial; sub-task 3 done, sub-task 2 deferred)

> Branch `modernize/phase2-net11`. Phase 8 deferred its sub-tasks 2 & 3 as "not blind-safe,
> no verification path." **Phase 15 then created that verification path** — `dotnet test` now
> runs, and it surfaced `DynoAudioViewModelTest` failing precisely on the sub-task-3 coupling
> (`GeneralSettings` → `Preferences.Default`). This follow-up does the **blind-safe,
> test-verifiable slice** of sub-task 3 and re-enables that test.

## Result

| Sub-task | Outcome |
|---|---|
| ~~Settings ↔ `Microsoft.Maui.Storage` coupling (Preferences)~~ | **Done (this phase).** `GeneralSettings` + `UnitSettings` now have the same design-time `Preferences` fallback that `DatabaseSettings` already had. `DynoAudioViewModelTest` re-enabled (green). |
| View `Ioc.Default` sites (23) / Shell→registered-routes | **Still deferred.** Device-only navigation restructure. |
| Full `IPlatformSettings` DI abstraction (instance `DatabaseSettings`, injected through `OnConfiguring`) | **Still deferred.** Wide ripple, device-only. This phase's fallback is the pragmatic interim. |

**Build:** MacCatalyst head **green, 0 errors**, warning set **unchanged** (CS0618/CS8981/MAUIG2045 —
the try/catch additions add no warnings). **Tests: 51 passed / 0 skipped / 0 failed** (was 46 + 1 skipped).

## Applied (this phase)

### The problem, precisely
`DynoAudioViewModel.FilterPlot` → `CheckDynoData` reads
`GeneralSettings.AudioAccelerationFilePath`, whose getter called
`Preferences.Default.Get(...)` **directly**. In a headless `net11.0` xUnit host there is no
MAUI Essentials platform implementation, so that throws
`NotImplementedInReferenceAssemblyException`. `DatabaseSettings` already tolerates this exact
case via try/catch fallbacks (`GetPreference`/`SetPreference`/`GetAppDataDirectory`) — which is
why the `DynoDataViewModelTest` DB path passed. `GeneralSettings` and `UnitSettings` were the
outliers.

### The fix — fallback parity (behavior-preserving on device)
- **`GeneralSettings`** — the 7 string properties now route through private
  `GetPreference`/`SetPreference` helpers that try `Preferences.Default` and return the
  documented default (get) / swallow (set) on `Exception`. This is a near-byte-identical mirror
  of `DatabaseSettings.GetPreference/SetPreference` (same signature, same try/catch, same
  comment). DRY win too: 14 inlined get/set bodies → 2 helpers.
- **`UnitSettings`** — the 2 typed properties (`int RoundingAccuracy`, `bool RoundOnUnitChange`)
  now route through generic `GetPreference<T>`/`SetPreference<T>` helpers with the same fallback.

**Why this is blind-safe and behavior-preserving:** on a real device `Preferences.Default` is
always available, the `try` succeeds, and the fallback branch never executes — the get/set
behavior is identical to before. The fallback only fires in design-time/headless hosts
(unit tests, XAML designer), where it converts a hard crash into the documented default. This is
the *same* pattern `DatabaseSettings` has shipped with since the original codebase.

### `DynoAudioViewModelTest` re-enabled
Removed the Phase-15 `[Fact(Skip = …)]`. With the fallback, the audio path resolves to a
non-existent file in the headless host, `CheckDynoData` returns `false`, and `FilterPlot`/
`RefreshPlot`/`SpecificGraph` gracefully no-op — the test now passes.

### `SettingsTest` (new)
Locks the fallback at the unit level: `GeneralSettings` returns its documented defaults; the
`*FilePath` properties compose with `DatabaseSettings.FileDirectory` without throwing; `Set` is
swallowed (a follow-up `Get` still returns the default); `UnitSettings` returns its defaults.
4 tests.

## Deferred (with rationale)

### Sub-task 2 — 23 `Ioc.Default` View call-sites (unchanged from Phase 8)
`MainPage` is a `Shell` with `ShellContent ContentTemplate="{DataTemplate …View}"`; Shell's
DataTemplate instantiates pages via their **parameterless** constructor, so DI constructor
injection requires abandoning `ContentTemplate` for `Routing.RegisterRoute` +
`Shell.Current.GoToAsync(...)` — a navigation-model overhaul across the flyout structure, **none
of it runtime-verifiable without a device/simulator**. Doing it blind would risk the app's entire
navigation. The service-locator call is the established MAUI Shell workaround for DI'd VMs on
DataTemplate-created pages. → still a dedicated, device-verified follow-up.

### Full `IPlatformSettings` DI abstraction (the architecturally pure end-state)
The fallback added here *tolerates* the `Microsoft.Maui.Storage` coupling; it doesn't remove it.
The full refactor — an `IPlatformSettings` interface, `GeneralSettings`/`DatabaseSettings`/
`UnitSettings` converted from `static` to instance + DI-injected, and the static
`DatabaseSettings.DatabasePath` read inside `DatabaseContext.OnConfiguring` replaced by an
options-lambda — is a wide ripple across every consumer plus non-trivial injection into EF
context creation. Phase-1 rates this **P3**. It remains the deferred end-state; the fallback is
the pragmatic, test-covered interim that removes the concrete blocker (hard crash in
tests/designer).

## Verification

- **`dotnet test src/SimTuning.Test -c Debug`** → **Failed: 0, Passed: 51, Skipped: 0, Total: 51.**
  (`DynoAudioViewModelTest` re-enabled and green; 4 new `SettingsTest` cases green.)
- **MacCatalyst head (`SimTuning.Maui.App -f net11.0-maccatalyst`)** — **0 errors**; warning set
  unchanged (CS0618/CS8981/MAUIG2045 — the try/catch helpers add none).
- **Behavior-preserving on device** — the fallback branch is unreachable when Essentials is present.

## Carry-forward

- Sub-task 2: Shell→registered-routes + device verification (removes the 23 `Ioc.Default` sites).
- Full `IPlatformSettings` DI abstraction (instance Settings, `OnConfiguring` options-lambda) — P3.
- Device smoke test of the Settings round-trip (Preferences actually persisted/loaded).
