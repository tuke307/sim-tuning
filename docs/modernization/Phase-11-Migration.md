# Phase 11 — Exception handling (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: replace empty/bare catches and the untyped
> `throw new Exception()` (Phase-1 §7.7) with typed exceptions and explicit catch
> filters. Scope is **small and behavior-preserving** — exception *types* become more
> specific and swallows become explicit, but propagation/default behavior is unchanged.

## Result

| Sub-task | Outcome |
|---|---|
| Untyped `throw new Exception()` | `NavigationService.Navigation` getter → `throw new InvalidOperationException("No active navigation stack is available.")` (Phase-1 §7.7). |
| Bare `catch { }` | 3 sites in `DatabaseSettings` → `catch (Exception)` (explicit filter; design-time fallback comments + behavior preserved). |

**Build:** MacCatalyst head builds **green, 0 errors, 0 new warnings** (12, unchanged).

## Applied (this phase)

### 1. `NavigationService` — typed exception (the §7.7 headline)

The `Navigation` getter threw a bare `throw new Exception();` (no type, no message) when
no window/page navigation was available. Replaced with a typed, descriptive
`InvalidOperationException`. The `Debugger.Break()` (when attached) is preserved; the
exception still propagates identically to callers (`Navigate<T>` / `NavigateBack` don't
catch it) — only the type and message improve.

```csharp
throw new InvalidOperationException("No active navigation stack is available.");
```

### 2. `DatabaseSettings` — bare catches → explicit filter

Three bare `catch { }` blocks (`GetAppDataDirectory`, `GetPreference`, `SetPreference`)
are intentional **design-time fallbacks** — when MAUI `Preferences`/`FileSystem` aren't
available (e.g. designer), they return a safe default (`Path.GetTempPath()` / the passed
default / no-op). They're now `catch (Exception)` (the idiomatic modern form; an explicit
filter reads as intentional rather than accidental). The fallback bodies and comments are
unchanged — behavior is byte-identical.

> A sweep confirmed these were the only bare catches outside deferred vendored/platform
> code. The ~20 VM/service `catch (Exception exc) { _logger.LogError(exc, "... {Message}", exc.Message); }`
> blocks are **already proper** — `{Message}` there is a *named template* with `exc.Message`
> as its argument (correct structured logging, distinct from the Phase-9 anti-pattern where
> `exc.Message` was the template itself). They are left untouched; narrowing them to specific
> exception types would risk letting previously-handled exceptions escape (a behavior change)
> for no clear benefit at the command-handler safety-net layer.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| `SFF.cs:101` `try { … } catch { }` (swallows `IndexOutOfRange`) | Vendored **Spectrogram** DSP code, replaced wholesale by the NuGet package. Churning it now is waste that gets deleted. | 17 |
| `AudioUtlis.cs` `catch { return false; }` + `catch (Exception) { }` (empty) | Windows-only **NAudio** path (`Mp3ToWav`/`AudioCopy`), replaced by NLayer. Same rationale. | 13 |

## Verification

- **Build (`/usr/local/share/dotnet/dotnet`, off PATH):** `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline). iOS head environmentally blocked (Phase-2).
- **No residual `throw new Exception`** (grep clean — 0).
- **No residual bare `catch`** in production code (only the deferred `AudioUtlis:47` + inline `SFF:101` remain, both documented above).
- **`DatabaseSettings`** now has 3× `catch (Exception)` (verified).
- Propagation behavior unchanged (the typed `InvalidOperationException` is thrown from the same path, caught by the same — i.e. no — callers).
- Warning baseline unchanged (12): `Styles_Global` CS0618, `AuslassAnwendungView` MAUIG2045, `resources` CS8981, `DynoRuntime` CS0414/CS0169.

## Carry-forward

- `SFF.cs:101` empty catch → Phase 17 (vendored Spectrogram replacement).
- `AudioUtlis.cs` empty/bare catches → Phase 13 (NAudio→NLayer replacement).
