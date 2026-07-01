# Release a New Version to NuGet

## 1. Update Version

Update the `<Version>` property in each `.csproj` file:

| Package | File |
|---|---|
| `InlayHtmlTemplate` | `src/InlayHtmlTemplate/InlayHtmlTemplate.csproj` |
| `InlayHtmlTemplate.DaisyUI` | `src/InlayHtmlTemplate.DaisyUI/InlayHtmlTemplate.DaisyUI.csproj` |
| `InlayHtmlTemplate.Components` | `src/InlayHtmlTemplate.Components/InlayHtmlTemplate.Components.csproj` |

Use [SemVer 2.0](https://semver.org/). Example: `0.2.0`, `1.0.0`, `1.0.1-beta`.

## 2. Test

```bash
dotnet test
```

## 3. Build Release Packages

```bash
dotnet pack src/InlayHtmlTemplate/InlayHtmlTemplate.csproj -c Release -o ./nupkg
dotnet pack src/InlayHtmlTemplate.DaisyUI/InlayHtmlTemplate.DaisyUI.csproj -c Release -o ./nupkg
dotnet pack src/InlayHtmlTemplate.Components/InlayHtmlTemplate.Components.csproj -c Release -o ./nupkg
```

This produces `.nupkg` and `.snupkg` (symbols) files in the `./nupkg` directory.

## 4. Push to NuGet

```bash
dotnet nuget push ./nupkg/*.nupkg --api-key YOUR_API_KEY --source https://api.nuget.org/v3/index.json
```

Replace `YOUR_API_KEY` with your NuGet API key.

Alternatively, store the key in an environment variable:

```bash
dotnet nuget push ./nupkg/*.nupkg --api-key $NUGET_API_KEY --source https://api.nuget.org/v3/index.json
```

## 5. Verify

- Check the package is listed at [nuget.org/packages/InlayHtmlTemplate](https://www.nuget.org/packages/InlayHtmlTemplate)
- Test installing from a clean project:
  ```bash
  dotnet new console -n test-install
  cd test-install
  dotnet add package InlayHtmlTemplate --version NEW_VERSION
  ```

## Prerequisites

- [NuGet account](https://www.nuget.org) with API key
- `dotnet` CLI (SDK 8.0+)
- Push permissions on the `InlayHtmlTemplate` NuGet packages
