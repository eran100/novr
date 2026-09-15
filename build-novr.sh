#!/usr/bin/env bash
set -euo pipefail

REPO_ROOT="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
cd "$REPO_ROOT"

# Windows Nuclear Option installation as seen from WSL.
export NUCLEAR_OPTION_GAME_DIR="${NUCLEAR_OPTION_GAME_DIR:-/mnt/c/Program Files (x86)/Steam/steamapps/common/Nuclear Option}"

echo "=== NOVR full build ==="
echo "Repository: $REPO_ROOT"
echo "Game:       $NUCLEAR_OPTION_GAME_DIR"
echo

# Verify required game files are accessible.
if [[ ! -d "$NUCLEAR_OPTION_GAME_DIR/NuclearOption_Data/Managed" ]]; then
    echo "ERROR: Nuclear Option Managed directory not found."
    exit 1
fi

if [[ ! -d "$NUCLEAR_OPTION_GAME_DIR/BepInEx/core" ]]; then
    echo "ERROR: BepInEx 5 core directory not found."
    exit 1
fi

echo "=== Building NOVR plugin ==="
dotnet build NOVR/NOVR.csproj \
    -c Release \
    -p:GameDeployPath=

echo
echo "=== Building NOVR patcher ==="
dotnet build NOVR.Patcher/NOVR.Patcher.csproj \
    -c Release \
    -p:GameDeployPath=

PLUGIN="$REPO_ROOT/build-output/plugins/NOVR.dll"
PATCHER="$REPO_ROOT/build-output/patchers/NOVR.Patcher.dll"
PAYLOAD="$REPO_ROOT/build-output/patchers/CopyToGame"

echo
echo "=== Verifying output ==="

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
echo "=== SHA-256 ==="
sha256sum "$PLUGIN" "$PATCHER"

echo
echo "=== BUILD SUCCESSFUL ==="
echo "Run the Windows Deploy-NOVR.ps1 script next."