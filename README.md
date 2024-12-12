# SimTuning

## Developing

download .NET 9.0 SKD from https://dotnet.microsoft.com/en-us/download/dotnet/9.0

## Setup

```bash
dotnet workload install maui maui-android maui-ios maui-maccatalyst
cd src/
dotnet build -t:Run -f net9.0-ios
dotnet build -t:Run -f net9.0-maccatalyst
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
