#!/usr/bin/env bash
set -euo pipefail

project_root="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
version="$(sed -n 's/.*<Version>\(.*\)<\/Version>.*/\1/p' "$project_root/PortalPass.csproj")"
package_root="$project_root/artifacts/package/PortalPass-$version"
archive="$project_root/artifacts/PortalPass-v$version.zip"

dotnet build "$project_root/PortalPass.csproj" --configuration Release
rm -rf "$package_root"
mkdir -p "$package_root"
cp "$project_root/artifacts/bin/Release/PortalPass.dll" "$package_root/"
cp "$project_root/README.md" "$project_root/CHANGELOG.md" "$project_root/LICENSE" "$project_root/manifest.json" "$project_root/icon.png" "$package_root/"
rm -f "$archive"
(cd "$package_root" && zip -q -r "$archive" .)
printf '%s\n' "$archive"
