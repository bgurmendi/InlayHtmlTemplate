#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")/.." && pwd)"
cd "$SCRIPT_DIR"

NUGET_API_KEY="${NUGET_API_KEY:-}"
NUGET_SOURCE="https://api.nuget.org/v3/index.json"
NUPKG_DIR="./nupkg"

PROJECTS=(
  "src/InlayHtmlTemplate/InlayHtmlTemplate.csproj"
  "src/InlayHtmlTemplate.DaisyUI/InlayHtmlTemplate.DaisyUI.csproj"
  "src/InlayHtmlTemplate.Components/InlayHtmlTemplate.Components.csproj"
)

usage() {
  echo "Usage: $0 [<new-version>] [--dry-run]"
  echo ""
  echo "Automates NuGet release: updates version, tests, packs, and pushes."
  echo ""
  echo "Arguments:"
  echo "  <new-version>   New SemVer version (e.g. 0.2.0, 1.0.0-beta)"
  echo "                  If omitted, the patch number is auto-incremented."
  echo "  --dry-run       Build and pack but skip push"
  echo ""
  echo "Environment:"
  echo "  NUGET_API_KEY   NuGet API key (required unless --dry-run)"
  exit 1
}

DRY_RUN=false
VERSION=""

for arg in "$@"; do
  case "$arg" in
    --dry-run) DRY_RUN=true ;;
    -*)
      echo "Unknown option: $arg"
      usage
      ;;
    *)
      if [[ -z "$VERSION" ]]; then
        VERSION="$arg"
      else
        echo "Unexpected argument: $arg"
        usage
      fi
      ;;
  esac
done

if [[ -z "$VERSION" ]]; then
  PROJ="${PROJECTS[0]}"
  CURRENT="$(grep -oP '<Version>\K[^<]+' "$PROJ")"
  BASE="${CURRENT%%[-+]*}"
  IFS='.' read -r MAJOR MINOR PATCH <<< "$BASE"
  VERSION="$MAJOR.$MINOR.$((PATCH + 1))"
  echo "    No version given: auto-incrementing $CURRENT -> $VERSION"
fi

if ! [[ "$VERSION" =~ ^[0-9]+\.[0-9]+\.[0-9] ]]; then
  echo "Error: version must be a valid SemVer (e.g. 0.2.0, 1.0.0-beta). Got: $VERSION"
  exit 1
fi

if [[ "$DRY_RUN" == false && -z "$NUGET_API_KEY" ]]; then
  echo "Error: NUGET_API_KEY is not set. Use --dry-run to skip push, or export NUGET_API_KEY."
  exit 1
fi

echo "==> Updating version to $VERSION"
for proj in "${PROJECTS[@]}"; do
  sed -i "s|<Version>[0-9.]*[^<]*</Version>|<Version>$VERSION</Version>|" "$proj"
  echo "    Updated $proj"
done

echo ""
echo "==> Running tests"
dotnet test

echo ""
echo "==> Cleaning old packages"
rm -rf "$NUPKG_DIR"
mkdir -p "$NUPKG_DIR"

echo ""
echo "==> Building and packing Release packages"
for proj in "${PROJECTS[@]}"; do
  dotnet pack "$proj" -c Release -o "$NUPKG_DIR"
done

echo ""
echo "Packages created:"
ls -1 "$NUPKG_DIR"/

if [[ "$DRY_RUN" == true ]]; then
  echo ""
  echo "Dry-run complete. Packages are in $NUPKG_DIR/"
  echo "To push them later, run:"
  echo "  dotnet nuget push $NUPKG_DIR/*.nupkg --api-key \"\$NUGET_API_KEY\" --source $NUGET_SOURCE"
  exit 0
fi

echo ""
read -r -p "Push these packages to NuGet.org? [y/N] " confirm
if [[ "$confirm" != "y" && "$confirm" != "Y" ]]; then
  echo "Aborted. Packages remain in $NUPKG_DIR/"
  exit 0
fi

echo ""
echo "==> Pushing to NuGet.org"
dotnet nuget push "$NUPKG_DIR"/*.nupkg --api-key "$NUGET_API_KEY" --source "$NUGET_SOURCE"

echo ""
echo "==> Verifying: installing InlayHtmlTemplate $VERSION in a temp project"
TMP_DIR="$(mktemp -d)"
pushd "$TMP_DIR" > /dev/null
dotnet new console -n verify-install --force > /dev/null 2>&1
cd verify-install
dotnet add package InlayHtmlTemplate --version "$VERSION" > /dev/null 2>&1
echo "    Package InlayHtmlTemplate $VERSION installed successfully."
popd > /dev/null
rm -rf "$TMP_DIR"

echo ""
echo "Release $VERSION complete!"
echo "Check https://www.nuget.org/packages/InlayHtmlTemplate"
