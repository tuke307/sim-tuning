# Phase 12 — DRY the UnitsNet `*Unit` boilerplate (✅ Complete)

> Branch `modernize/phase2-net11`. Goal: collapse the ~46× duplicated unit-conversion
> setter boilerplate in the Data models into one helper (Phase-1 §3 "massive duplication;
> `AuspuffModel.cs` (1,047 LOC) is ~90% this boilerplate"). Scope is **mechanical and
> behavior-preserving** — the conversion logic moves into a shared helper verbatim; each
> setter shrinks from ~13 lines to ~2.

## Result

| Sub-task | Outcome |
|---|---|
| `ConvertValueForUnit` helper | Added to `BaseEntityModel` (`protected static`); reproduces the inlined `TryConvert` + `RoundOnUnitChange` logic exactly. |
| `*Unit` setter dedup | **45 live setters** across 8 model files collapsed. **811 deletions / 74 insertions (net −737 LOC)**; `AuspuffModel` 1,047 → ~670. |

**Build:** MacCatalyst head builds **green, 0 errors, 0 new warnings** (12, unchanged);
Test project compiles (0 errors). Data is `Nullable=disable`, so no nullable fallout.

## Applied (this phase)

### 1. `ConvertValueForUnit` helper (`BaseEntityModel`)

Every `*Unit` setter had the identical conversion body — only the field/unit names varied:

```csharp
set
{
    if (this.AbgasT.HasValue)
    {
        UnitsNet.UnitConverter.TryConvert(this.AbgasT.Value, this.AbgasTUnit, value, out double convertedValue);
        this.AbgasT = UnitSettings.RoundOnUnitChange
            ? Math.Round(convertedValue, UnitSettings.RoundingAccuracy)
            : convertedValue;
    }
    this._AbgasTUnit = value;
}
```

Extracted verbatim into:

```csharp
protected static double? ConvertValueForUnit(double? value, Enum fromUnit, Enum toUnit)
{
    if (!value.HasValue) return value;
    UnitsNet.UnitConverter.TryConvert(value.Value, fromUnit, toUnit, out double convertedValue);
    return UnitSettings.RoundOnUnitChange
        ? Math.Round(convertedValue, UnitSettings.RoundingAccuracy)
        : convertedValue;
}
```

> **Signature note:** the params are `Enum` (not a generic `<TUnit> where TUnit: struct, Enum`).
> The unit properties are nullable (`TemperatureUnit?`, `LengthUnit?`, …) and a `Nullable<TEnum>`
> boxes to `Enum` exactly as it did in the inlined setters — so the underlying
> `TryConvert(double, Enum, Enum, out double)` call is byte-identical. A generic constrained to
> `Enum` would *not* accept the nullable unit types.

Each setter now reads:

```csharp
set
{
    this.AbgasT = ConvertValueForUnit(this.AbgasT, this.AbgasTUnit, value);
    this._AbgasTUnit = value;
}
```

### 2. Applied across 8 model files (45 setters)

| Model | Setters DRY'd |
|---|---|
| `AuspuffModel` | 20 |
| `MotorModel` | 8 |
| `EinlassModel` | 5 |
| `AuslassModel` | 5 |
| `UeberstroemerModel` | 3 |
| `EnvironmentModel` | 2 |
| `VehiclesModel` | 1 |
| `VergaserModel` | 1 |

The transformation was done with a **conservative regex script** (`/tmp/dry_units.py`, not
committed): a block was only rewritten if it matched the *exact* token sequence
(`if HasValue → TryConvert → if RoundOnUnitChange {Round} else {plain} → _XUnit = value`)
with the **same property name throughout** (enforced via backreferences). One "miss" was
`VehiclesModel`'s commented-out `FrontA` stub (dead code) — correctly left untouched.

**Behavior-preserving:** the helper makes the same `TryConvert` call with the same argument
types and the same `RoundOnUnitChange`/`RoundingAccuracy` policy; `TryConvert`'s ignored bool
result and the `!HasValue → unchanged` path are preserved exactly. The conversion/rounding
policy is now centralized in one place (previously editing it meant touching 45 sites).

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| `AuslassLogic` CS8629 `#pragma` removal (Phase 6 carry-forward) | Needs non-null propagation across `Calculate`/`ComputeGeometry` or a typed-geometry path — fiddly, orthogonal to the DRY. The pragma is localized + documented (identical cast IL). | follow-up |
| `[ObservableProperty]` sweep (Phase 6 carry-forward) | Limited applicability: `VehiclesViewModelBase` mirrors delegate to the model (no backing field) → source-gen doesn't apply. | follow-up |
| `EnsureDatabaseCreated` `Migrate()`+`EnsureCreated()` anti-pattern (Phase-1 §7.4) | DB-init behavior change; needs device verification of migration/creation. | follow-up (needs device) |
| `Styles_Global` ListView→CollectionView (CS0618 ×6) | XAML control migration → layout change needing visual review on device (like Phase 4's `*AndExpand` deferral). | follow-up (needs device) |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Data` — **0 errors, 0 warnings** (Nullable=disable; clean).
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline: `Styles_Global` CS0618 ×6, `AuslassAnwendungView` MAUIG2045 ×3, `resources` CS8981, `DynoRuntime` CS0414/CS0169).
  - `SimTuning.Test` — **0 errors** (model changes didn't break test compilation).
  - iOS head environmentally blocked (Phase-2).
- **LOC:** `src/SimTuning.Data/Models/` **811 deletions / 74 insertions** (net −737); `AuspuffModel` 1,047 → ~670.
- **45/45 live setters transformed** (1 commented-out dead-code stub correctly skipped).
- **No `UnitsNet.UnitConverter.TryConvert` calls remain in model setters** (only in the helper + the dead commented stub).
- No automated tests cover the unit-conversion behavior (`dotnet test` 0-discovered — Phase 15). Confidence rests on the behavior-preserving extraction (identical `TryConvert` call + policy, verified by build + diff review) + the static checks above.

## Carry-forward

- `AuslassLogic` CS8629 pragma removal (non-null propagation / typed geometry).
- `[ObservableProperty]` sweep (limited applicability).
- `EnsureDatabaseCreated` `Migrate()`+`EnsureCreated()` → pick one (needs device).
- `Styles_Global` ListView→CollectionView (needs device visual review).
- Device smoke test: change a unit on a quantity field in the UI (exercises `ConvertValueForUnit` end-to-end).
