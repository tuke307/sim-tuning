# Phase 14 — Security hardening (✅ Partial; 2 items deferred)

> Branch `modernize/phase2-net11`. Goal: the Phase-1 §6 security findings — Zip-Slip on
> archive extraction (A01), deprecated `SecureString` (A02), `Newtonsoft.Json` deserialize
> surface (A08), path canonicalization (A01), and an encrypt-at-rest decision (A02). The
> high-value, self-contained fixes are done; the two that need a decision / are intertwined
> with deferred work are deferred with rationale.

## Result

| Sub-task | Outcome |
|---|---|
| Zip-Slip defense (A01) | `ImportDyno` now validates each archive entry's canonicalized path before extraction; entries escaping `FileDirectory` are rejected. |
| `SecureString` removal (A02) | Dead `SecureStringToString` / `StringToSecureString` deleted (0 external refs; deprecated API). |
| `Newtonsoft.Json` → `System.Text.Json` (A08) | `DynoDataViewModel` export (active) + import (commented) migrated to `System.Text.Json` (`ReferenceHandler.Preserve`); **`Newtonsoft.Json` package dropped.** |
| Path canonicalization (A01) | **Deferred** — intertwined with the static `DatabaseSettings`/`GeneralSettings` decoupling (Phase 8b). |
| Encrypt-at-rest decision (A02) | **Deferred** — needs a threat-model + key-management decision. |

**Build:** MacCatalyst head **green, 0 errors, 0 new warnings** (12, unchanged); Test **0 errors**.

## Applied (this phase)

### 1. Zip-Slip defense in `ImportDyno` (A01)

`ImportDyno` downloads an archive from a URL and extracted it with
`ZipFile.ExtractToDirectoryAsync`, which performs **no per-entry validation** — a crafted
archive (entries like `../../etc/...`) could write outside the target directory (Zip Slip).
Replaced with an explicit per-entry loop that canonicalizes each target path and rejects any
that don't stay rooted under `FileDirectory`:

```csharp
string fullDestinationDirectory = Path.GetFullPath(destinationDirectory) + Path.DirectorySeparatorChar;
foreach (ZipArchiveEntry entry in archive.Entries)
{
    string fullTarget = Path.GetFullPath(Path.Combine(fullDestinationDirectory, entry.FullName));
    if (!fullTarget.StartsWith(fullDestinationDirectory, StringComparison.Ordinal))
        throw new InvalidOperationException($"Refusing to extract zip entry '{entry.FullName}' — it escapes the target directory.");
    ...
    entry.ExtractToFile(fullTarget, overwrite: true);
}
```

- The trailing `Path.DirectorySeparatorChar` on the prefix prevents the classic bypass
  (target `app/` matching entry `app-evil/...`).
- **AsyncFixer02** (sync `OpenRead`/`ExtractToFile` in an async method) is suppressed with a
  **localized, documented `#pragma`**: per-entry validation can't use `ExtractToDirectoryAsync`,
  and the calls are fast local-file IO (low deadlock risk).
- **Behavior note:** extraction now uses `overwrite: true` (the previous
  `ExtractToDirectory` threw `IOException` on any pre-existing file). The import flow deletes
  stale files first anyway, so overwrite is the more robust behavior.

> The archive URL is currently a hardcoded, first-party sample
> (`simtuning.tony-luke.de/.../DataExport.zip`), so the live exploitability is low — but the
> validation is correct defense-in-depth and future-proofs any user-supplied archive source.

### 2. Remove deprecated `SecureString` (A02)

`Converts.SecureStringToString` / `StringToSecureString` used `SecureString` + `Marshal`
(SYSLIB-deprecated; `SecureString` is not recommended on modern .NET). A grep confirmed
**0 external references** — they were dead. Deleted both (plus the now-unused
`System.Security` / `System.Runtime.InteropServices` usings). `SKBitmapToStream` (the third
method, used by 2 ViewModels) is kept. (`RNGCryptoServiceProvider`, the other Phase-1 A02
finding, was already removed in Phase 4.)

### 3. `Newtonsoft.Json` → `System.Text.Json` (A08) + drop the dependency

The sole `Newtonsoft.Json` consumer was `DynoDataViewModel`:
- `ExportDynoAsync` — `JsonConvert.SerializeObject(Dyno, PreserveReferencesHandling.Objects)` (active).
- `ImportDyno` — `JsonConvert.DeserializeObject<DynoModel>(json)` (in a commented-out `/* TODO: only for testing */` block).

Both migrated to `System.Text.Json` with a shared, cached `JsonSerializerOptions`
(`WriteIndented`, `ReferenceHandler.Preserve` — the STJ equivalent of Newtonsoft's
`PreserveReferencesHandling.Objects`, handling the `Dyno↔Vehicle` reference loop). The
**`Newtonsoft.Json 13.0.4` `PackageReference` is removed** from `src/Directory.Build.props`
(verified no other consumer). A08 (deserializing downloaded JSON) was dormant — the
deserialize path is commented — but the migration makes it STJ-safe when activated and drops a
dependency.

> **Breaking (export format):** the exported `DataExport.json` now uses the System.Text.Json
> format (`$id`/`$values`/`$ref` metadata) instead of Newtonsoft's. No code round-trips the
> file today (import's deserialize is commented), so there's no functional impact; external
> consumers of the export should re-parse against the new format.
>
> Two StyleCop warnings the first attempt introduced (SA1000 target-typed `new()`, SA1204
> static-member order — Phase 5 flagged target-typed `new()` as SA1000-blocked) were fixed:
> explicit `new JsonSerializerOptions()` and the static field placed before instance fields.

## Deferred (with rationale)

| Item | Reason | Phase |
|---|---|---|
| Path canonicalization (A01) | The `Preferences`-backed paths (`DatabaseSettings.FileDirectory`/`DatabasePath`, `GeneralSettings.*FilePath`) are app-controlled defaults, so live risk is low; the real fix is intertwined with the static-settings → injected-abstraction decoupling (Data↔MAUI.Storage). | 8b |
| Encrypt-at-rest (A02) | Plaintext SQLite stores lat/long (`AusrollenModel`). Encrypting needs a decision: AES with a key from the platform keychain/keystore? Per-user or device key? Scope (whole DB or PII columns only)? Not blind-safe; needs the threat model agreed first. | decision |
| `SFF` untrusted-binary parse (A06) | Resolved by deletion — the vendored `SFF.cs` (no bounds checks) was deleted in Phase 17. | ✅ (via P17) |

## Verification

- **Builds (`/usr/local/share/dotnet/dotnet`, off PATH):**
  - `SimTuning.Maui.App -f net11.0-maccatalyst -c Debug` — **0 errors, 12 warnings** (unchanged baseline).
  - `SimTuning.Test` — **0 errors** (STJ migration + Newtonsoft removal compile clean).
  - iOS head environmentally blocked (Phase-2).
- **`Newtonsoft.Json`** — no code/package reference remains (only a doc-comment mention).
- **`SecureString`** — no references remain (grep clean).
- **Zip-Slip** — the extraction loop canonicalizes + prefix-checks every entry; `overwrite: true`.
- No automated test covers the import/export path (`AudioLogicTest`/etc. are Phase 15). Confidence rests on the build + the static checks; a device smoke test of Export→Share→Import is recommended.

## Carry-forward

- Path canonicalization (with the Phase-8b static-settings decoupling).
- Encrypt-at-rest decision (threat model + key management).
- Device smoke test: Export dyno → Share → Import (exercises STJ serialize + Zip-Slip-validated extraction).
