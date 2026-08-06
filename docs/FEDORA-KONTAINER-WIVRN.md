# Fedora KDE + Kontainer/Distrobox + Proton + WiVRn Build Guide

This guide documents a tested development setup for building and running NOVR
on **Fedora KDE 44** using a **Kontainer / Distrobox** container for
development, **Steam + Proton** for running Nuclear Option on the host, and
**WiVRn Flatpak** for OpenXR streaming to a standalone headset.

Commands assume the repository is at `~/Projects/novr` and the game is at
`~/.local/share/Steam/steamapps/common/Nuclear Option`.

The interactive shell inside the container is **Fish**, but all commands are
given in a form that also works in Bash where possible.  Where Fish and Bash
syntax differ, both variants are shown.

---

## 1. Prerequisites

Install these inside the **container**:

| Package                     | Provides                                |
| --------------------------- | --------------------------------------- |
| `git`                       | Repository clone                        |
| `dotnet-sdk-9` (or later 9) | .NET SDK for `dotnet build`             |
| `mono-core` / `mono-msbuild`| MSBuild and .NET Framework 4.8 tooling  |
| `mono-reference-assemblies` | .NET Framework 4.8 reference assemblies |
| `binutils`                  | Provides `monodis`                      |

`gh` (GitHub CLI) is **optional**; it is only needed for GitHub release
workflows and is not required to build or run NOVR.

On the **host** (Fedora KDE 44), install:

* Steam (RPM or Flatpak)
* Nuclear Option (via Steam)
* [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases/latest) installed
  inside the Nuclear Option game directory
* [WiVRn Flatpak](https://github.com/WiVRn/WiVRn)

---

## 2. Confirming the Development Shell Is Inside the Container

Even though your shell prompt may still display the host's hostname, you can
verify you are inside the Distrobox / Kontainer container:

```bash
test -f /run/.containerenv && echo "Inside container" || echo "On host"
```

If `test` prints nothing, the condition is **false** and you are on the host.
All build commands must be run from **inside** the container.

---

## 3. Locating Nuclear Option

### Automatic detection

The build system (`NOVR.Build/NOVR.Sources.props`) checks these paths in
order and uses the first one where
`NuclearOption_Data/Managed` exists:

1. `$HOME/Locations/NuclearOption/Install`
2. `~/.steam/steam/steamapps/common/Nuclear Option`
3. `~/.steam/debian-installation/steamapps/common/Nuclear Option`
4. `~/.local/share/Steam/steamapps/common/Nuclear Option`

### Manual override

If none of those paths match your installation, set the
`NUCLEAR_OPTION_GAME_DIR` environment variable or pass the MSBuild property:

```fish
# Fish
set -x NUCLEAR_OPTION_GAME_DIR "/path/to/Nuclear Option"
```

```bash
# Bash
export NUCLEAR_OPTION_GAME_DIR="/path/to/Nuclear Option"
```

---

## 4. Restoring the Repository

```bash
dotnet restore NuclearOptionVirtualRealityMod.sln
```

A benign warning about skipping `Uuvr.XInput.vcxproj` is expected on Linux.
That project is a Windows-only C++ library and is not needed for the main
plugin or patcher.

---

## 5. Building the Main Plugin

```bash
dotnet build NOVR/NOVR.csproj -c Release
```

### Output

| Artifact  | Path                            |
| --------- | ------------------------------- |
| Built DLL | `build-output/plugins/NOVR.dll` |

### Deployment

The build system automatically deploys the DLL into the game directory at:

```
BepInEx/plugins/NOVR/NOVR.dll
```

(relative to the detected Nuclear Option game directory).

### Verify deployment

```bash
cmp \
  build-output/plugins/NOVR.dll \
  "$NUCLEAR_OPTION_GAME_DIR/BepInEx/plugins/NOVR/NOVR.dll" \
  && echo "Deployed DLL matches build" \
  || echo "DLLs differ — was the build or deploy step skipped?"
```

If `cmp` reports a mismatch, re-run the build while the game is **closed**
(the build system skips unchanged files).

---

## 6. Building the Required Patcher

The patcher must be built and deployed **at least once**.  Without it, BepInEx
loads NOVR but the Unity player is never initialised as a proper OpenXR
application, and VR will not function.

```bash
dotnet build NOVR.Patcher/NOVR.Patcher.csproj -c Release
```

### Output

| Artifact  | Path                                      |
| --------- | ----------------------------------------- |
| Built DLL | `build-output/patchers/NOVR.Patcher.dll`  |

### Deployment

```
BepInEx/patchers/NOVR/NOVR.Patcher.dll
```

**Important:** If the `BepInEx/patchers` directory is missing or empty, BepInEx
loads NOVR without the patcher.  VR initialisation will be silently skipped.

---

## 7. Patcher Payload

The patcher ships a `CopyToGame/` payload that is deployed alongside
`NOVR.Patcher.dll` into `BepInEx/patchers/NOVR/CopyToGame/`.  At game
startup the patcher copies these files into the game's `NuclearOption_Data`
tree.

| Payload file                                                              | Purpose                                    |
| ------------------------------------------------------------------------- | ------------------------------------------ |
| `CopyToGame/Data/Managed/Unity.XR.Management.dll`                          | XR management layer                        |
| `CopyToGame/Data/Managed/Unity.XR.OpenXR.dll`                              | OpenXR loader plugin                       |
| `CopyToGame/Plugins/x64/openxr_loader.dll`                                 | Native OpenXR loader                       |
| `CopyToGame/Plugins/x64/UnityOpenXR.dll`                                   | Unity OpenXR native plugin                 |
| `CopyToGame/Data/UnitySubsystems/UnityOpenXR/UnitySubsystemsManifest.json` | Registers OpenXR display + input providers |
| `CopyToGame/Data/UnitySubsystems/XRSDKOpenVR/UnitySubsystemsManifest.json` | Registers OpenVR display + input providers |
| `CopyToGame/Data/StreamingAssets/SteamVR/`                                 | SteamVR action and binding files           |

These files only need to be rebuilt and recopied when the patcher itself or
the XR dependency assemblies change.  Normal NOVR plugin work does **not**
require touching them.

---

## 8. Proton File-Lock Workaround

Under Proton the patcher may fail to overwrite files inside
`NuclearOption_Data` and log an error such as:

> The process cannot access Unity.XR.Management.dll because it is being used
> by another process.

This happens because Proton holds open file handles to the game's data
directory while Steam is running, even when Nuclear Option is not actively
playing.

### Manual deployment procedure

Run this **only while Nuclear Option is completely closed** (the Steam client
may remain open):

```fish
# Fish
set game "$HOME/.local/share/Steam/steamapps/common/Nuclear Option"
set patcher "$game/BepInEx/patchers/NOVR"

cp -av \
    "$patcher/CopyToGame/Data/." \
    "$game/NuclearOption_Data/"

cp -av \
    "$patcher/CopyToGame/Plugins/x64/." \
    "$game/NuclearOption_Data/Plugins/"
```

```bash
# Bash
game="$HOME/.local/share/Steam/steamapps/common/Nuclear Option"
patcher="$game/BepInEx/patchers/NOVR"

cp -av \
    "$patcher/CopyToGame/Data/." \
    "$game/NuclearOption_Data/"

cp -av \
    "$patcher/CopyToGame/Plugins/x64/." \
    "$game/NuclearOption_Data/Plugins/"
```

Once these files are in place, ordinary changes confined to `NOVR.dll` do not
require rebuilding or recopying the patcher payload.  The build system
deploys `NOVR.dll` directly to `BepInEx/plugins/NOVR/`.

---

## 9. WiVRn Steam Launch Options

Configure Nuclear Option's Steam launch options:

```text
PRESSURE_VESSEL_IMPORT_OPENXR_1_RUNTIMES=1 PRESSURE_VESSEL_FILESYSTEMS_RW=/var/lib/flatpak/app/io.github.wivrn.wivrn OXR_RECENTER_STAGE=1 WINEDLLOVERRIDES="winhttp=n,b" gamemoderun %command%
```

| Fragment                                                                    | Required? | Purpose                                                    |
| --------------------------------------------------------------------------- | --------- | ---------------------------------------------------------- |
| `PRESSURE_VESSEL_IMPORT_OPENXR_1_RUNTIMES=1`                                | Yes       | Imports the host OpenXR runtime into the Proton container  |
| `PRESSURE_VESSEL_FILESYSTEMS_RW=/var/lib/flatpak/app/io.github.wivrn.wivrn` | Yes       | Grants read-write access to the WiVRn Flatpak installation |
| `WINEDLLOVERRIDES="winhttp=n,b"`                                            | Yes       | Required for BepInEx to load under Proton                  |
| `OXR_RECENTER_STAGE=1`                                                      | No        | Recenters the stage on launch (convenience)                |
| `gamemoderun`                                                               | No        | Enables Feral's GameMode optimisations                     |

---

## 10. Correct Startup Sequence

1. **Start WiVRn** on the host (`flatpak run io.github.wivrn.wivrn` or via
   your desktop launcher).
2. **Connect the headset** and confirm the WiVRn dashboard shows it as
   connected.
3. **Launch Nuclear Option** from Steam.
4. The NOVR in-game menu may initially appear **behind you** in VR.  Turn
   around physically, or use the `VR CENTER` button in the game's VR settings.
5. In the NOVR menu, set **`Enable Native Menu UI = true`**.  This enables
   the in-cockpit VR UI and is recommended for the best experience.

---

## 11. Logs and Troubleshooting

### BepInEx log

```
Nuclear Option/BepInEx/LogOutput.log
```

Search it for NOVR and OpenXR messages:

```bash
rg -i "novr|openxr" "BepInEx/LogOutput.log"
```

### Proton / Unity player log

For Steam AppID `2168680`:

```
~/.local/share/Steam/steamapps/compatdata/2168680/pfx/drive_c/users/steamuser/AppData/LocalLow/Mitch Games/Nuclear Option/Player.log
```

Search with:

```bash
rg -i "novr|openxr|xr" ~/.local/share/Steam/steamapps/compatdata/2168680/pfx/drive_c/users/steamuser/AppData/LocalLow/Mitch\ Games/Nuclear\ Option/Player.log
```

### Successful OpenXR startup indicators

When NOVR initialises correctly you will see all of these in the logs:

* `OpenXR display provider registered`
* `OpenXR input provider registered`
* `openxr_loader` loaded
* Session reaches `XR_SESSION_STATE_FOCUSED`

If any of these is missing, re-check that the patcher payload files were
successfully deployed into `NuclearOption_Data` (see section 8).

---

## 12. Development Workflow

### First-time / after XR dependency changes

Build the full patcher once so the payload files are correctly placed:

```bash
dotnet build NOVR.Patcher/NOVR.Patcher.csproj -c Release
```

If Proton blocks automatic deployment, apply the manual copy procedure from
section 8.

### Ongoing NOVR feature work

For normal C# changes to the plugin behaviour:

```bash
dotnet build NOVR/NOVR.csproj -c Release
```

The build system deploys `NOVR.dll` to `BepInEx/plugins/NOVR/` automatically.

### Safe deploy checklist

* Keep Nuclear Option **closed** while deploying.
* If you rebuilt the patcher, run the manual copy from section 8 if needed.
* Rebuild only `NOVR/NOVR.csproj` for routine plugin changes.
* Test a clean baseline launch before implementing additional features.
