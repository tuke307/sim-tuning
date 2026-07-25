# SimTuning Modernization — Living Docs

Continuous documentation for the .NET 10 → .NET 11 modernization (branch `v2.0.0`).
Per the roadmap, docs are **seeded in Phase 1 and refreshed every phase** — not written at the end.

## Index

| Doc | Purpose |
|---|---|
| [Phase-1-Analysis.md](./Phase-1-Analysis.md) | Full project analysis: structure, per-project findings, security, .NET 11 risk register, TODO → phases |
| [Phase-2-Migration.md](./Phase-2-Migration.md) | .NET 11 TFM migration log: changes, decisions, verification, environment setup |

## Phase 2 — ✅ Complete (migrate TFMs to .NET 11)

**Outcome:** all 6 projects build on .NET 11 (`11.0.100-preview.6`). The full **MacCatalyst desktop app builds green**, and every library compiles on `net11.0`, `net11.0-ios`, and `net11.0-maccatalyst`. The only remaining build failure is `SimTuning.Maui.App` on `net11.0-ios`, blocked by a **local iOS simulator-runtime mismatch** (environmental, pre-existing — the README already documents iOS simulator issues), fixable with `xcodebuild -downloadPlatform iOS`.

**Beyond the brief's scope (justified):** the migration exposed that `SimTuning.Test` inherited mobile TFMs (so it was being built as an iOS/Mac *app bundle*). The proper fix — pin Test to `net11.0`-only and give `Core`/`Maui.UI`/`Spectrogram` a `net11.0` base target — was done here rather than deferred, because it's required for the solution to build and is the conventional MAUI test setup. Test *contents* (assertions, cross-platform paths, missing `xunit.runner.visualstudio` adapter) remain Phase 15.

See [Phase-2-Migration.md](./Phase-2-Migration.md) for the full change list and decisions.

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
- [ ] **Phase 3** — Maui.Controls/EF Core → preview; validate LiveCharts/SkiaSharp/MediaElement/Sharpnado/FftSharp/DBSCAN net11 builds; **plan NAudio cross-platform replacement** (Windows-only today); make `WinUI.Notifications` Windows-conditional; NU1903 vulns (SQLitePCLRaw 2.1.11, System.Security.Cryptography.Xml 9.0.0) should clear via EF Core → 11 preview.
- [ ] **Phase 4** — install csharpier+prettier; fix real analyzer hits (CS8618 ×300, CS8603 ×272, CS8605 ×168, CS0618 ×92, SA1027 ×48, …) — no new `NoWarn`.
- [ ] **Phase 6** — break up `VehiclesViewModelBase` (1,318 LOC) + `AuslassLogic.Auspuff` (460-LOC method); fix duplicate popup registration; standardize source generators.
- [ ] **Phase 8** — `DatabaseContext` Singleton→Scoped; remove 22 `Ioc.Default` View call-sites; decouple `SimTuning.Data` from `Microsoft.Maui.Storage`.
- [ ] **Phase 9** — drop Serilog `Verbose` floor in Release.
- [ ] **Phase 14** — zip-entry validation + System.Text.Json; path canonicalization; encrypt-at-rest decision; remove `RNGCryptoServiceProvider`/`SecureString`.
- [ ] **Phase 15** — add `xunit.runner.visualstudio`; real assertions; fix dead tests (`AudioLogicTest` missing `[Fact]`, `DynoLogicTest` commented); cross-platform test paths; async tests.
- [ ] **Phase 17** — replace vendored `Spectrogram` with NuGet (sole consumer `AudioLogic` confirmed; dead render/SFF/Colormap surface documented).

Full detail and rationale: see [Phase-1-Analysis.md](./Phase-1-Analysis.md) §6–§9.
