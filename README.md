# SimTuning

## Developing

download .NET 10.0 SKD from https://dotnet.microsoft.com/en-us/download/dotnet/10.0

## Setup

```bash
sudo dotnet workload restore
cd src/
dotnet build
```

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
dotnet build -f net10.0-maccatalyst -p:RuntimeIdentifier=maccatalyst-arm64

# Open the app
open bin/Debug/net10.0-maccatalyst/maccatalyst-arm64/SimTuning.app
```

#### iOS Simulator (requires compatible Xcode/SDK)

```bash
cd src/SimTuning.Maui.App
# List available simulators and boot one
xcrun simctl list devices available | grep iPhone
xcrun simctl boot <SIMULATOR_UDID>

# Build for iOS simulator (requires iOS SDK 18.2+)
dotnet build -f net10.0-ios -p:RuntimeIdentifier=iossimulator-arm64

# Note: Currently having SDK compatibility issues with iOS 18.2 simulator
# Recommended: Use VS Code with .NET MAUI extension for iOS development
```

## format documents

npx prettier --write "**/\*.cs"
npx prettier --write "**/\*.xaml"

# Platforms

| Platform | Tested | Deployed |
| -------- | ------ | -------- |
| Windows  | ✅     | ✅       |
| macOS    | ✅     | ✅       |
| iOS      | ✅     | ✅       |
