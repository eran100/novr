#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

GAME="${NUCLEAR_OPTION_GAME_DIR:-$HOME/.local/share/Steam/steamapps/common/Nuclear Option}"
export NUCLEAR_OPTION_GAME_DIR="$GAME"

PLUGIN="$REPO_ROOT/build-output/plugins/NOVR.dll"
PATCHER="$REPO_ROOT/build-output/patchers/NOVR.Patcher.dll"
PAYLOAD="$REPO_ROOT/build-output/patchers/CopyToGame"

DEPLOYED_PLUGIN="$GAME/BepInEx/plugins/NOVR/NOVR.dll"
DEPLOYED_PATCHER="$GAME/BepInEx/patchers/NOVR/NOVR.Patcher.dll"

echo "=== NOVR Fedora build ==="
echo "Repository: $REPO_ROOT"
echo "Game:       $GAME"
echo

if [[ ! -d "$GAME/NuclearOption_Data/Managed" ]]; then
    echo "ERROR: Nuclear Option was not found at:"
    echo "  $GAME"
    exit 1
fi

if [[ ! -d "$GAME/BepInEx/core" ]]; then
    echo "ERROR: BepInEx 5 was not found in the Nuclear Option directory."
    exit 1
fi

if pgrep -af "[N]uclearOption.exe|[N]uclear Option" >/dev/null; then
    echo "ERROR: Nuclear Option appears to be running."
    echo "Close the game before rebuilding/deploying NOVR."
    exit 1
fi

if ! command -v dotnet >/dev/null; then
    echo "ERROR: dotnet was not found."
    exit 1
fi

echo "dotnet: $(dotnet --version)"
echo

echo "=== Building NOVR plugin ==="
dotnet build NOVR/NOVR.csproj -c Release

echo
echo "=== Building NOVR patcher ==="
dotnet build NOVR.Patcher/NOVR.Patcher.csproj -c Release

echo
echo "=== Verifying build outputs ==="

[[ -f "$PLUGIN" ]] || {
    echo "ERROR: NOVR.dll was not produced."
    exit 1
}

[[ -f "$PATCHER" ]] || {
    echo "ERROR: NOVR.Patcher.dll was not produced."
    exit 1
}

[[ -d "$PAYLOAD" ]] || {
    echo "ERROR: CopyToGame payload was not produced."
    exit 1
}

echo "NOVR.dll:         OK"
echo "NOVR.Patcher.dll: OK"
echo "CopyToGame:       OK"

echo
echo "=== Deploying XR payload ==="

cp -a "$PAYLOAD/Data/." "$GAME/NuclearOption_Data/"

mkdir -p "$GAME/NuclearOption_Data/Plugins"
cp -a "$PAYLOAD/Plugins/x64/." "$GAME/NuclearOption_Data/Plugins/"

echo
echo "=== Verifying deployment ==="

cmp "$PLUGIN" "$DEPLOYED_PLUGIN"
echo "NOVR.dll deployed correctly"

cmp "$PATCHER" "$DEPLOYED_PATCHER"
echo "NOVR.Patcher.dll deployed correctly"

echo
echo "=== SHA-256 ==="
sha256sum "$PLUGIN" "$PATCHER"

echo
echo "=== BUILD AND DEPLOY SUCCESSFUL ==="
