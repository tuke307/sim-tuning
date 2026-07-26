# Phase 7 — Async/await modernization (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: clear the **AsyncFixer02** sync-IO-in-async
> warnings, migrate the cross-thread `System.Timers.Timer` to a UI-thread
> `IDispatcherTimer`, remove the one non-event-handler `async void`, thread a
> `CancellationToken` into the cancellable network path, and harden the remaining
> `async void` event handlers. Scope is **behavior-preserving**; public
> property/command names that XAML binds to are unchanged.

## Result

| Sub-task | Outcome |
|---|---|
| AsyncFixer02 sync-IO (the 2 deferred warnings) | `DynoDataViewModel`: `File.WriteAllText`→`WriteAllTextAsync`; `ZipFile.ExtractToDirectory`→`ExtractToDirectoryAsync`. **2 warnings → 0.** |
| `System.Timers.Timer` cross-thread | `DynoRuntimeViewModel` now uses `IDispatcherTimer` (UI thread). The `Elapsed`→`Tick` handlers no longer mutate bound properties off-thread. |
| Non-event-handler `async void` | `Functions.ShowSnackbarDialog` (`async void`) → `ShowSnackbarDialogAsync` (`async Task`); 9 call sites → explicit `_ =` fire-and-forget. |
| `async void` event-handler hardening | `NavigationService.Page_Navigated{To,From}` wrapped in try/catch + logged (injected `ILogger<NavigationService>`). |
| `CancellationToken` | `IBrowserService.DownloadDocumentAsync` + impl take an optional `CancellationToken` (flowed to `GetStreamAsync`/`CopyToAsync`). |

**Build:** MacCatalyst head builds **green, 0 errors, 0 new warnings**
(14 → 12 unique warnings — the 2 AsyncFixer02 cleared; no AsyncFixer03/04 or CS4014
introduced). iOS head still environmentally blocked (Phase-2).

## Applied (this phase)

### 1. AsyncFixer02 — `DynoDataViewModel` (the Phase-4 deferral, now cleared)

The two sync-IO warnings that Phase 4 explicitly deferred to P7:

- `ExportDynoAsync` — `File.WriteAllText(path, json)` → `await File.WriteAllTextAsync(path, json)` (method already `async Task`).
- `ImportDyno` — `ZipFile.ExtractToDirectory(src, dest)` → `await ZipFile.ExtractToDirectoryAsync(src, dest)` (static async overload; .NET Core 2.1+). No `ZipFile.OpenRead`/`ZipArchive` split — that path re-introduces an AsyncFixer02 on `OpenRead`; the single static call is cleanest.

> **Scope note:** the *security* hardening of `ImportDyno` (per-entry zip validation, `Newtonsoft.Json`→`System.Text.Json`) stays in **Phase 14** — Phase 7 only does the mechanical sync→async conversion.

### 2. `DynoRuntimeViewModel` timer → `IDispatcherTimer` (the P2 risk-register item)

`System.Timers.Timer.Elapsed` fires on a **thread-pool thread**; the handlers
(`OnCountdownTimedEvent`, `OnStopwatchTimedEvent`) call `OnPropertyChanged(...)` and
flip bound visibility flags → cross-thread mutation of bound properties (the exact
bug class this phase targets). `IDispatcherTimer` ticks **on the UI thread**, so the
same mutations are now safe.

- `private System.Timers.Timer? timer` → `private IDispatcherTimer? timer` (`using Microsoft.Maui.Dispatching;`).
- A `CreateDispatcherTimer(double intervalMs, EventHandler tick)` helper builds the timer via `Application.Current!.Dispatcher.CreateTimer()` and wires `Interval` + `Tick`. The 3 creation sites (`StartAusrollen`, `StartAcceleration`, `StartStopwatch`) call it with their original intervals (10 ms countdown ×2, 100 ms stopwatch).
- `IDispatcherTimer` is **not `IDisposable`**, so the 4 `timer.Stop(); timer.Dispose();` sites became `timer.Stop();` (Stop is sufficient — the timer stops firing; GC reclaims it).
- Handler signatures `ElapsedEventArgs` → `EventArgs` (`IDispatcherTimer.Tick` is `EventHandler`); neither handler used the arg.

> **Dispatcher access:** `Application.Current.Dispatcher` is used (no constructor
> change) so `MauiViewModelsTest`'s direct `new DynoRuntimeViewModel(...)` still
> compiles unchanged. `Application.Current` is guaranteed non-null in the running
> app — the only context these `[RelayCommand]` methods execute in (tests never
> reach the timer-creation path; `CheckDynoData` fails on the permission mock first).
>
> **Timing precision:** `IDispatcherTimer` is less real-time-precise than
> `System.Timers.Timer`, but the countdown already relied on the same 10 ms tick and
> the stopwatch display reads `Stopwatch.Elapsed` (high-precision). Net timing for
> the display is equivalent; the cross-thread bug is fixed.

### 3. `Functions.ShowSnackbarDialog` — `async void` → `async Task`

The single **non-event-handler** `async void` (the clear "remove async void" target —
the 4 popup + 2 navigation handlers are legitimate event handlers and are handled
separately). Renamed `ShowSnackbarDialog` → `ShowSnackbarDialogAsync`, return `Task`.
A snackbar is inherently fire-and-forget UI, so all **9 call sites** use an explicit
`_ = Functions.ShowSnackbarDialogAsync(...)` discard — which preserves the prior
fire-and-forget semantics, suppresses CS4014 ("call not awaited"), and (confirmed by
the build) does **not** trip AsyncFixer04.

### 4. `NavigationService` async-void handlers — exception-safety

Event handlers cannot escape `async void` (their signature is fixed), so the
documented fix is to make them exception-safe: an unhandled throw in an `async void`
**terminates the process**. `Page_NavigatedFrom` and `Page_NavigatedTo` bodies are now
wrapped in `try/catch` that logs via an injected `ILogger<NavigationService>`. DI
supplies the logger (Serilog bridge already registered); `MauiViewModelsTest` mocks
`INavigationService` and never constructs `NavigationService`, so no test change.

> The empty `throw new Exception();` in the `Navigation` getter (Phase-1 §7.7) is
> **not** touched — typed exceptions are Phase 11's scope. Phase 7 only prevents a
> navigation-handler exception from crashing the app.

### 5. `CancellationToken` on the network download

`IBrowserService.DownloadDocumentAsync` (and `BrowserService`) now take an optional
`CancellationToken cancellationToken = default`, flowed to `HttpClient.GetStreamAsync`
and `Stream.CopyToAsync`. Optional/defaulted → the one caller (`ImportDyno`) needs no
change, and the `Mock<IBrowserService>` test is unaffected. The network download is
the natural cancellation target; `OpenBrowser` (`Browser.Default.OpenAsync`) has no
CT overload and is left as-is.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| `DynoAudioViewModel` sync `[RelayCommand]` over CPU work (FFT/clustering/polyfit) | A correct offload needs thread-marshaling: the commands mutate the UI-bound `PlotAudio` `ObservableCollection` *interleaved* with the heavy compute, and `AudioLogic` carries **static mutable state** (`SpectrogramAudio`, etc.) that isn't thread-safe. `Task.Run`-ing the whole method would mutate the bound collection off-thread (the very bug class this phase fixes elsewhere); splitting compute/marshal-back is a real refactor with **no automated tests** to guard it (`dotnet test` is 0-discovered — Phase 15). | follow-up (after AudioLogic thread-safety + Phase-15 tests) |
| Popup `Button_Clicked` `async void` (×2) | Trivial single-`await CloseAsync(...)` handlers delegating to framework code; no logger in the popup, and converting would inject loggers into 2 popup ctors for negligible risk. Empty-catch/typed-exception cleanup is Phase 11. | 11 |
| `ConfigureAwait(false)` sweep | AsyncFixer03 is **not firing** (not enabled). MAUI guidance is the opposite — **do not** add `ConfigureAwait(false)` in ViewModels (bindings need the UI sync context). The handful of existing `.ConfigureAwait(true)` calls are no-ops (true is default) and left as-is. | — (intentional) |
| `ImportDyno` per-entry zip validation + `System.Text.Json` | Security hardening, separate concern. | 14 |
| `AudioUtils` `File.Delete` / `Mp3ToWav` | Synchronous methods (not `async`) → no AsyncFixer02; and the NAudio Windows-only path → cross-platform replacement. | 13 |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst` — **0 errors, 0 new warnings.** Unique warnings **14 → 12** (the 2 AsyncFixer02 cleared). Remaining 12 are all pre-existing: `MAUIG2045` (DiffusorStage binding), `CS0618` (Styles_Global ListView), `CS8981` (resources), `CS0414`/`CS0169` (DynoRuntime dead fields).
  - `SimTuning.Test` — **0 errors** (23 pre-existing test warnings, Phase 15). Confirms the `ShowSnackbarDialogAsync` rename, `NavigationService` ctor, and `IBrowserService` CT param didn't break test compilation.
  - iOS head environmentally blocked (Phase-2), ignored.
- **No AsyncFixer03/04 or CS4014 introduced** — the `_ =` discard pattern at the 9 snackbar call sites suppresses both, confirmed by the clean rebuild.
- **No `ShowSnackbarDialog(` (old name) references remain** (grep clean); all 9 + 1 internal callers migrated.
- **No `new NavigationService(` manual constructions** (DI-only) — so the added `ILogger` ctor param needs no call-site changes.
- **No `System.Timers` / `ElapsedEventArgs` / `timer.Dispose()` residuals** in `DynoRuntimeViewModel` (grep clean).
- **No automated VM tests** (`dotnet test` 0-discovered, adapter missing — Phase 15). Confidence rests on the behavior-preserving nature of each change + the static invariants above. A device smoke test of the dyno run (countdown → stopwatch → reset) is recommended to confirm the `IDispatcherTimer` UI-thread timing feels right.

## Carry-forward

- Device smoke test: dyno acceleration flow (countdown → stopwatch → reset) exercises the `IDispatcherTimer` migration; the Import/Export dyno flow exercises the async file/zip IO.
- `DynoAudioViewModel` CPU offload (after `AudioLogic` static-state thread-safety + Phase-15 tests).
- Popup `async void` try/catch once loggers are available (Phase 11).
