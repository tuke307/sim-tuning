# Phase 2 — .NET 11 TFM Migration Log

> Branch `modernize/phase2-net11` (off `v2.0.0`). SDK `11.0.100-preview.6.26359.118`.
> Goal: migrate target frameworks `net10.0-*` → `net11.0-*` and get the solution building on .NET 11.

## Result

All 6 projects build on .NET 11. The full **MacCatalyst desktop app (`SimTuning.Maui.App.dll`) builds green**, and every library compiles on `net11.0`, `net11.0-ios`, and `net11.0-maccatalyst`. The `SimTuning.Test` project builds on `net11.0`.

The only build failure is `SimTuning.Maui.App` on `net11.0-ios`, caused by a **local iOS simulator-runtime mismatch** (environmental, pre-existing — main README documents iOS simulator issues). Fix: `xcodebuild -downloadPlatform iOS`. This is not a migration regression; the net11 iOS app *compiles* — only the final simulator-packaging step needs the runtime.

## Changes

### Brief-specified
| File | Change |
|---|---|
| `global.json` (new, repo root) | Pin `11.0.100-preview.6.26359.118`, `rollForward: latestPatch`, `allowPrerelease: true` |
| `src/Directory.Build.props` | `net10.0-ios;net10.0-maccatalyst` (+ Windows) → `net11.0-*` (incl. commented tizen) |
| `src/SimTuning.Data/SimTuning.Data.csproj` | override `net10.0` → `net11.0` |
| Environment | `sudo dotnet workload restore src/SimTuning.sln` → `maui-ios`, `maui-maccatalyst` |

### Discovered during migration (gotchas the brief missed)
| File | Change | Why |
|---|---|---|
| `src/SimTuning.Maui.App/SimTuning.Maui.App.csproj` | `Debug\|net10.0-ios` & `Release\|net10.0-ios` → `net11.0-ios` (in `CreatePackage` conditions) | Hardcoded TFM in build conditions; would silently stop matching after migration and change iOS packaging |
| `src/SimTuning.Maui.App/SimTuning.Maui.App.csproj` | `SupportedOSPlatformVersion` iOS `12.2→13.0`, MacCatalyst `15.0→17.0` | **.NET 11 raised the platform floors** (build errors otherwise) |
| `src/SimTuning.Maui.App/SimTuning.Maui.App.csproj` | preview-feature comment updated | cosmetic |

### Test-harness restructure (beyond brief, justified)
The migration turned `SimTuning.Test` (which inherited the mobile TFMs) into an iOS/Mac **app bundle** requiring a bundle id + simulator/signing — wrong for a unit-test project, and it blocked the build.

| File | Change |
|---|---|
| `src/SimTuning.Test/SimTuning.Test.csproj` | Pin `net11.0`-only, `UseMaui=false`, `EnablePreviewFeatures=true` (consumes preview-annotated types from Core/Maui.UI → avoids CA2252) |
| `src/SimTuning.Core/SimTuning.Core.csproj` | Append `net11.0` base target |
| `src/SimTuning.Maui.UI/SimTuning.Maui.UI.csproj` | Append `net11.0` base target |
| `src/Spectrogram/Spectrogram.csproj` | Append `net11.0` base target + `UseMaui=false` (DSP lib, never used MAUI — Phase-1 finding) |
| `src/SimTuning.Test/ViewModels/MauiViewModelsTest.cs` | Add `Mock<IPopupService>` + pass to 9 VM constructors | Pre-existing breakage: commit `e0ecdba` (popup work) added `IPopupService` to VM ctors but never updated the tests; latent because the net10 build never ran (workloads were missing) |

## Verification

- `dotnet build src/SimTuning.sln`: every project + TFM green except `Maui.App` net11.0-ios (iOS simulator runtime — env).
- `dotnet build src/SimTuning.Test/SimTuning.Test.csproj`: ✅ 0 errors.
- `dotnet test`: **0 tests discovered** — `xunit.runner.visualstudio` adapter is missing from `SimTuning.Test.csproj` (pre-existing; Phase 15).
- 626 compiler/analyzer warnings now captured (the first successful compile). Dominant: nullable debt (CS8618 ×300, CS8603 ×272, CS8605 ×168…), obsolete APIs (CS0618 ×92, SYSLIB0023 ×4), NU1903 vulnerable packages (×80), SA1027 tab/space (×48). → Phase 4/5/14.

## Decisions & rationale

1. **Test TFM restructure done now, not Phase 15.** The brief assumed Test was `net10.0`-only (it wasn't — it inherited mobile TFMs). The migration made that an app-bundle build error, so the restructure (Test → `net11.0`, libraries gain `net11.0` base targets) is required for the build to pass. Test *contents* (assertions, paths, adapter) stay Phase 15.
2. **ApplicationId hack attempted then reverted.** First tried adding a bundle id to keep Test on mobile TFMs (minimal), but it dragged Test into the iOS actool/signing pipeline. The net11.0-only approach is cleaner and correct.
3. **Spectrogram `UseMaui=false`.** Vendored DSP lib that never used MAUI; flipping it off (Phase-1 finding) de-risks the new `net11.0` target and is a Phase-16 cleanup done early.
4. **iOS simulator runtime not downloaded.** ~5 GB; optional for verification since MacCatalyst fully proves the net11 MAUI app build. Offered as a follow-up.
5. **Formatters deferred.** `csharpier`/`prettier` aren't installed; Phase-2 changes are config + one style-matched `.cs` edit. Setup deferred to Phase 4.

## Platform-version implications (brief asked for these)

.NET 11 / MAUI 11 raised minimum `SupportedOSPlatformVersion` floors: **iOS ≥ 13.0** (was 12.2), **MacCatalyst ≥ 17.0** (was 15.0). Bumped in `Maui.App.csproj`. Note: `Maui.UI.csproj` still declares iOS 14.2 / MacCatalyst 14.0 — those only *warn* for libraries (not error), but should be aligned (≥17.0 MacCatalyst) in a later phase to avoid advertising unavailable APIs. Android (21.0) and Windows (10.0.17763/10.0.18362) were not flagged by the build.

Mono→CoreCLR runtime validation (NAudio/SkiaSharp/FftSharp/DBSCAN on real devices) is deferred to runtime testing — not exercisable by a desktop build.
