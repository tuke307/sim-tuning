# Phase 17 — Spectrogram (vendored) cleanup (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: replace the ~5-year-old vendored Spectrogram
> (copied from `Spectrogram 2.0.0-alpha` / v1) with the NuGet if it fits, else refactor the
> vendored copy and delete the dead surface. **Outcome: the NuGet was evaluated and rejected
> (Windows-only, image-focused, unmaintained); the vendored copy was kept and trimmed from
> 995 → 318 LOC.** Behavior of the consumed API is preserved.

## Result

| Sub-task | Outcome |
|---|---|
| NuGet evaluation | **Rejected.** Modern `Spectrogram` NuGet depends on `System.Drawing.Common` (Windows-only), is image-focused (no `GetFFTs()` raw-data API), and is unmaintained since 2022. |
| Dead surface deletion | **4 files deleted** (`SFF`, `Image`, `Colormap`, `Tools` = 534 LOC) + `Spectrogram.cs` trimmed (266 → ~110). |
| Modernization | `AllowUnsafeBlocks` removed (was unused); `SkiaSharp` dependency dropped from the code; `WavFile` console-spam removed; `AudioLogicTest` repointed to the kept API. |

**Build:** MacCatalyst head **green, 0 errors, 0 new warnings** (12, unchanged); Test **0 errors**.
**Spectrogram project: 995 → 318 LOC (−677).**

## NuGet evaluation (why not replace)

The consumed API (from `AudioLogic`, the sole consumer) is small and data-oriented:

```csharp
(_, double[] audio) = Spectrogram.WavFile.ReadMono(path);
var sg = new Spectrogram.Spectrogram(sampleRate, fftSize, stepSize, minFreq, maxFreq);
sg.Add(audio, process: true);
double hzPerPx   = sg.HzPerPx;
double secPerPx  = sg.SecPerPx;
List<double[]> ffts = sg.GetFFTs();   // raw FFT magnitude columns — the core need
```

The modern **`Spectrogram` NuGet (swharden)** was rejected on four grounds:

1. **API mismatch (the decisive reason).** The NuGet's main type is `SpectrogramGenerator` with `SaveImage`/`GetBitmap`; it does **not** expose `GetFFTs()` raw FFT data, which is exactly what `AudioLogic` reads to find rotational-speed hot points. It also has no public `HzPerPx`/`SecPerPx` or `WavFile.ReadMono`. Even a cross-platform NuGet wouldn't fit this data-oriented consumer.
2. **Platform / maturity of the published package.** The latest *stable* on NuGet is **1.6.1 (July 2022)**, which depends on **`System.Drawing.Common`** — unsupported at runtime on iOS/MacCatalyst/Linux since .NET 6 (and the unix-support switch was removed in .NET 7). `dotnet add package Spectrogram` pulls this version. The `master`/`2.0.0-alpha` source *did* drop `System.Drawing` for SkiaSharp (cross-platform), but that rewrite is **unreleased** (no stable since 2022).
3. **No `WavFile.ReadMono`** in the package (the README shows it as user-authored example code using NAudio — also Windows-only).
4. **Unmaintained** — ~40K total downloads, no stable release in 3+ years.

Alternatives considered: rewriting `AudioLogic` directly on **FftSharp** (already a dep) + a hand-rolled WAV reader would eliminate the vendored project entirely, but that's a behavioral rewrite of the FFT/windowing/band-slicing pipeline with **no audio test** to verify identical output — too risky vs. trimming. Per the brief's fallback, the vendored copy is kept and cleaned.

## Applied (this phase)

### 1. Deleted dead files (534 LOC)

| File | LOC | Why dead |
|---|---|---|
| `SFF.cs` | 297 | `.sff` reader/writer — never called by the app; had the untrusted-binary parse (no bounds checks) + empty `catch {}` (Phase-1 §6 A06). |
| `Image.cs` | 119 | `GetBitmap`/`ApplyColormap` — only used by the dead `Spectrogram.GetBitmap*` methods. |
| `Tools.cs` | 90 | MIDI/piano-key + SFF helpers — only consumed `SFF` (deleted). |
| `Colormap.cs` | 28 | `Colormap` enum — only used by the deleted image/SFF surface. |

Verified 0 external references (the `Image.`/`Tools.` grep hits were false positives — `SKImage`, `System.Resources.Tools`).

### 2. Trimmed `Spectrogram.cs` (266 → ~110 LOC)

Kept only the consumed surface: the ctor, `Add`, `Process` (windowed-FFT magnitude extraction), `GetFFTs`, and the `HzPerPx` / `SecPerPx` / `Width` / `Height` properties. Removed: `GetBitmap`/`GetBitmapMax`/`GetBitmapMel`, `GetMelFFTs`, `GetPeak`, `PixelY`, `RollReset`, `SaveData`, `SetColormap`, `SetFixedWidth`, `SetWindow`, `ToString`, the `[Obsolete]` `AddCircular`/`AddExtend`/`AddScroll`, the `cmap`/`fixedWidth`/`rollOffset` fields, `PadOrTrimForFixedWidth`, and the now-unused `FftSize`/`SampleRate`/`StepSize`/`FreqMin`/`FreqMax`/`OffsetHz`/`FftsProcessed` exposures. `Process`'s core (`Parallel.For` windowed `FftSharp.FFT.Forward` + band-slice) is byte-identical — the consumed behavior is preserved.

> The ctor drops the unused `fixedWidth`/`offsetHz` params (AudioLogic never passed them); `FftsToProcess` is now `private`.

### 3. Modernization

- **`AllowUnsafeBlocks=true` removed** from `Spectrogram.csproj` (Phase-1 flagged it as set-but-unused).
- **`SkiaSharp` dropped from the code** — `using SkiaSharp;` and all `SKBitmap` usage lived in the deleted `GetBitmap*`/`Image.cs`. The vendored lib no longer references SkiaSharp at the source level (the transitive package may still flow through the inherited mobile TFMs, but the code is Skia-free).
- **`WavFile.cs`** — 10 `Console.WriteLine` debug-spam lines removed (they logged raw WAV header details to the console on every read). The WAV/PCM parsing logic is unchanged.
- **`AudioLogicTest.SpectrogramCreationTest`** — repointed from the deleted `GetBitmap()`/PNG-render path to the kept `GetFFTs()` API (`Assert.NotEmpty(...)`). Still missing `[Theory]` + uses a Windows-only path — both Phase 15's scope (left as-is; just kept compiling).

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline: `Styles_Global` CS0618 ×6, `AuslassAnwendungView` MAUIG2045 ×3, `resources` CS8981, `DynoRuntime` CS0414/CS0169).
  - `SimTuning.Test` — **0 errors** (confirms the `AudioLogicTest` rewrite + Spectrogram refactor compile; the project's pre-existing test warnings are Phase 15).
  - Spectrogram builds on `net11.0` (Test head) and the mobile TFMs (Core head) — both green.
  - iOS head environmentally blocked (Phase-2).
- **LOC:** Spectrogram project **995 → 318** (−677): 4 files deleted (−534) + `Spectrogram.cs` 266→~110 + `WavFile.cs` −10.
- **No external references** to any deleted symbol (grep clean; the apparent `Image.`/`Tools.` hits were `SKImage`/`System.Resources.Tools`).
- **Consumed API preserved** — `WavFile.ReadMono`, `Spectrogram` ctor/`Add`/`HzPerPx`/`SecPerPx`/`GetFFTs` are all present with identical behavior; `Process`'s FFT core is unchanged.
- No automated audio test (`AudioLogicTest` is still skipped — Phase 15). Confidence rests on the behavior-preserving trim (the kept API + `Process` core are unchanged) + the build across both TFMs + the static reference checks.

## Carry-forward

- Optional: rewrite `AudioLogic` directly on `FftSharp` (+ a cross-platform WAV reader) to eliminate the vendored project entirely — needs an audio-output equivalence test.
- `AudioLogicTest` activation (`[Theory]` + cross-platform path) → Phase 15.
