# SimTuning

A .NET MAUI app for two-stroke engine simulation & dyno tuning (exhaust/resonance, intake,
engine, and dyno/audio modules). Targets **.NET 11** (`net11.0-ios`, `net11.0-maccatalyst`,
`net11.0-windows`, `net11.0-android`).

> The codebase was recently migrated from .NET 10 to .NET 11 (TFMs, NuGet, modern C#, DI,
> async, security hardening, dead-code removal, and a real test suite). See
> [PR #108](https://github.com/tuke307/sim-tuning/pull/108) for the full modernization summary.

## Developing

This project targets the **.NET 11 SDK** (currently the `11.0.100-preview.6` preview).
`global.json` pins the exact version (`allowPrerelease: true`), so install that SDK and the
workloads below.

- SDK: <https://dotnet.microsoft.com/en-us/download/dotnet/11.0>
- `global.json` → `"version": "11.0.100-preview.6.26359.118"`

## Setup

```bash
sudo dotnet workload restore     # installs maui-ios, maui-maccatalyst
cd src/
dotnet build
```

> On macOS, also make sure Xcode is selected (`sudo xcode-select -s
> /Applications/Xcode.app/Contents/Developer`) and its license accepted
> (`sudo xcodebuild -license accept`).

## Running the App

### Using Visual Studio Code (Recommended)

1. Install the **.NET MAUI** extension in VS Code
2. Press `F5` or use the Run menu
3. Select target: macOS, iOS Simulator, or iOS Device

### Command Line

#### macOS (Mac Catalyst)

```bash
cd src/SimTuning.Maui.App
# Build the app
dotnet build -f net11.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64

# Open the app
open bin/Debug/net11.0-maccatalyst/maccatalyst-arm64/SimTuning.app
```

#### iOS Simulator (requires compatible Xcode/SDK)

```bash
cd src/SimTuning.Maui.App
# List available simulators and boot one
xcrun simctl list devices available | grep iPhone
xcrun simctl boot <SIMULATOR_UDID>

# Build for iOS simulator
dotnet build -f net11.0-ios -p:RuntimeIdentifier=iossimulator-arm64
```

> **Note:** the `net11.0-ios` app build needs an iOS simulator runtime that matches the
> installed Xcode. If the iOS build fails with a simulator-runtime mismatch, run
> `xcodebuild -downloadPlatform iOS`. The MacCatalyst target builds green without it.

## Tests

The `SimTuning.Test` project is a plain `net11.0` xUnit suite (51 tests) — no MAUI host
required, so it runs in the headless test runner:

```bash
dotnet test src/SimTuning.Test
```

## Formatting

C# is formatted with **csharpier** (config in `.csharpierrc.json`, `useTabs: false`); XAML and
other files use **prettier** (config in `.prettierrc.json`).

```bash
# C# — csharpier (global tool: `dotnet tool install -g csharpier`)
dotnet csharpier .

# XAML / JSON / Markdown — prettier
npx prettier --write "**/*.xaml"
```

> ⚠️ Do **not** run `csharpier` recursively on the whole tree if a `prettier` is also in scope —
> a phantom `prettier` can reformat ~70 unrelated C# files. Format the C# and XAML sets
> separately as shown above.

# Platforms

| Platform | Tested | Deployed |
| -------- | ------ | -------- |
| Windows  | ✅     | ✅       |
| macOS    | ✅     | ✅       |
| iOS      | ✅     | ✅       |
