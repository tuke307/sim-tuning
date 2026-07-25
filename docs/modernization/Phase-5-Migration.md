# Phase 5 — Use Modern C# (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: apply modern C# features **where they improve readability**, without changing behavior.

## Result

Phase 5 was deliberately **scoped and low-churn**. The codebase already enables the "big three" — `<Nullable>enable</Nullable>`, `<ImplicitUsings>enable</ImplicitUsings>`, `<LangVersion>latest</LangVersion>` — and Phase 4 already applied modern nullable idioms (`is not X` patterns, `?? default`, `?.`, switch expressions, `GetValueOrDefault`). The remaining modern-C# surface is small, and most of it either conflicts with later phases or is blocked by existing analyzer config. So Phase 5 = a focused, reviewable batch + honest documentation of what's deferred and why.

## Applied (this phase)

- **`using` declarations**: replaced block-form `using (var x = …) { … }` with `using var x = …;` where the block is the rest of the method (identical scope/disposal, less nesting):
  - `BrowserService.DownloadDocumentAsync` — three nested `using` blocks → three `using var` declarations.
  - `VehicleService.DeleteOne(VehiclesModel)` — `using (var db = …)` block → `using var db`.

## Deferred / not-applicable (and why)

| Feature | Decision | Reason |
|---|---|---|
| **File-scoped namespaces** | **Converge per-file in later phases** | ~180 of 181 `.cs` files use block-scoped namespaces. A blanket sweep is huge churn that conflicts with the Phase 6 (MVVM) and Phase 12 (DRY) rewrites. Better to convert files as they're touched. (Can be force-converted later via `csharp_style_namespace_declarations = file_scoped` in `.editorconfig` + `dotnet format`, if a one-shot churn commit is wanted.) |
| **`global usings`** | Not added | `<ImplicitUsings>enable</ImplicitUsings>` already provides the standard usings. The per-file `using` directives are localized inside namespaces (SA1200 disabled); consolidating further is churn with little gain. |
| **`record` / `init` / `required`** | Not applicable now | Domain models are EF Core entities (mutated by the context — `init`/`required` would break materialization); the Messenger DTOs (`CurrentDynoRequestMessage`, `DynoChangedMessage`) inherit framework message classes (a `record` can't derive from a non-record class). Revisit after Phase 8 (Data decoupling) / Phase 12. |
| **Target-typed `new()`** | Blocked | StyleCop **SA1000** ("keyword `new` should be followed by a space") fires on `new()`. The repo's existing code uses explicit types. (Disabling SA1000 in `.editorconfig` would enable `new()` but trades a style rule for a feature.) |
| **Primary constructors** | Skipped | Divisive style; low readability gain for the DI-ctor services. The MVVM VMs are slated for Phase 6 source-generator rewrite, so touching their ctors now is wasted. |
| **Pattern matching / switch expressions** | Applied opportunistically in Phase 4 | Already used (`is not VehiclesModel vehicle`, converter pattern-matches, `ShowPermissionDeniedMessage` switch expression). No large clunky switch statements remain in stable (non-god-class) code. |
| **Structural modernization** | Phase 6 / 12 | The highest-value "readable modern C#" wins (source-generating the `VehiclesViewModelBase` mirrors, breaking up god-classes, DRY-ing the UnitsNet boilerplate) are **structural** and belong to Phase 6 (MVVM) and Phase 12 (cleanup), not this syntactic phase. |

## Verification
- `dotnet build SimTuning.Core` (net11.0): **0 errors**, unchanged warning count (the using-declaration change is behavior-neutral).
- No behavior change — `using var` has identical disposal scope to the original `using` blocks here (each block was the remainder of its method).

## Carry-forward
- File-scoped-namespace conversion: do per-file as Phase 6/12 touch files, OR a dedicated `dotnet format` sweep if a one-shot churn commit is acceptable.
- Revisit `record`/`init` for models after Phase 8 (Data decoupling from MAUI Storage) and Phase 12.
