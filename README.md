# [Nuclear Option](https://store.steampowered.com/app/2168680/Nuclear_Option/) VR

NOVR is a reworked version of [UUVR](https://github.com/Raicuparta/uuvr) designed and optimized for Nuclear Option.

## User Installation

### Recommended: GUI installer

1. Close Nuclear Option before installing or updating the mod.
2. Download the latest installer from the [NOVR releases page](https://github.com/InfernoSuperNova/novr/releases/latest):
    - **Windows:** `NOVR.Installer-Win.exe`
    - **Linux/Proton:** `NOVR.Installer-Linux`
3. Run the installer directly.
    - On Linux, you may need to make it executable first: `chmod +x NOVR.Installer-Linux`.
4. If Nuclear Option is not found automatically, choose the game folder manually.
    - The selected folder must contain `NuclearOption_Data/Managed`.
5. Click **Install**.
6. Launch Nuclear Option from Steam.

The installer can also update, repair, or uninstall NOVR after it is installed.

### What the installer does

The installer:

- Finds your Nuclear Option install.
- Installs BepInEx 5.x if it is missing.
- Downloads the latest NOVR release zip.
- Installs NOVR into:
    - `BepInEx/plugins/NOVR`
    - `BepInEx/patchers/NOVR`
- Writes the installed NOVR version to `BepInEx/plugins/NOVR/version.txt`.
- On Linux/Proton, attempts to configure the `winhttp` Wine override needed by BepInEx.

Use BepInEx 5.x only. Do not use BepInEx 6.x unless the project explicitly says it is supported.

### Manual zip install

Use this only if the installer does not work for your setup.

1. Close Nuclear Option.
2. Install [BepInEx 5.x](https://github.com/BepInEx/BepInEx/releases/latest) into the Nuclear Option game folder.
    - After installing BepInEx, this folder should exist: `Nuclear Option/BepInEx/core`.
3. Download `NOVR.zip` from the [latest NOVR release](https://github.com/InfernoSuperNova/novr/releases/latest).
4. Extract the contents of `NOVR.zip` into `Nuclear Option/BepInEx`.
5. Confirm these files exist:

    ```text
    Nuclear Option/BepInEx/plugins/NOVR/NOVR.dll
    Nuclear Option/BepInEx/patchers/NOVR/NOVR.Patcher.dll
    ```

6. Launch Nuclear Option from Steam.

`NOVR.zip` contains `plugins` and `patchers` folders. Extract it into the existing `BepInEx` folder, not directly into the game root.

For a **locally built** `NOVR.zip` (for example, from the WSL2 build below), you do not need the GitHub-release installer. With Nuclear Option closed, extract the ZIP and copy its `plugins` and `patchers` folders directly into the game's existing `BepInEx` folder. If NOVR is not installed yet, this simply creates `BepInEx/plugins/NOVR` and `BepInEx/patchers/NOVR`; it does not require replacing BepInEx itself. Before replacing an existing NOVR install, back up those two NOVR folders.

**Back up before the first launch:** The patcher copies bundled XR files into `NuclearOption_Data` when the game starts and may replace/remove conflicting VR native DLLs under `NuclearOption_Data/Plugins`. Keep copies of existing files in `NuclearOption_Data/Plugins`, `NuclearOption_Data/Managed` (especially `Unity.XR.OpenXR.dll` and `Unity.XR.Management.dll`), `NuclearOption_Data/UnitySubsystems`, and `NuclearOption_Data/StreamingAssets/SteamVR` if present. Save backups outside the game folder. Removing NOVR's two BepInEx folders alone does **not** undo files previously copied into `NuclearOption_Data`; restore your backups when uninstalling, or use Steam's file verification for changed stock game files (which does not necessarily remove extra mod files).

### First launch behavior

On game startup, the NOVR BepInEx patcher copies required XR support files into `NuclearOption_Data`. These files may be overwritten every time the game starts.

If the game is already running while installing or rebuilding, Windows may prevent those files from being replaced. Close Nuclear Option before installing, updating, or building the mod.

### Linux/Proton notes

The installer tries to set the required `winhttp` override automatically. If BepInEx does not load under Proton, configure the game's Wine prefix manually so `winhttp` uses `native,builtin`.

See the [BepInEx running under Proton guide](https://docs.bepinex.dev/articles/advanced/proton_wine.html) for the manual setup.

## Building

These steps are for developers building NOVR from source.

1. Install a .NET/MSBuild toolchain that can build SDK-style .NET Framework 4.8 projects.
    - **Windows:** Visual Studio 2022 or Build Tools for Visual Studio 2022 with the **.NET Framework 4.8 targeting pack** installed.
    - **Linux:** the .NET SDK plus Mono/MSBuild and .NET Framework reference assemblies. Distro package names vary, but you usually want packages such as `dotnet-sdk`, `mono`, `msbuild`/`mono-msbuild`, and `mono-reference-assemblies`/`.NET Framework 4.8 reference assemblies`.
2. Have Nuclear Option installed at:
    - **Linux:** `~/.steam/steam/steamapps/common/Nuclear Option` OR `~/.steam/debian-installation/steamapps/common/Nuclear Option` OR `~/.local/share/Steam/steamapps/common/Nuclear Option`
    - **Windows:** `C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option` OR `C:\Program Files\Steam\steamapps\common\Nuclear Option` OR `D:\SteamLibrary\steamapps\common\Nuclear Option`

    If none of these options work for you, set the `NUCLEAR_OPTION_GAME_DIR` environment variable or pass `/p:NuclearOptionGameDir="path\to\Nuclear Option"` when building.
3. Ensure BepInEx 5.x is installed inside the Nuclear Option directory.
4. Build the `Release` configuration from your IDE of choice. JetBrains Rider is tested; Visual Studio should work too. To build from the command line, run this from the project root:

    ```bash
    dotnet build NuclearOptionVirtualRealityMod.sln -c Release
    ```

    The build output is written under `build-output`, as well as copied directly to the BepInEx directory.

For a step-by-step Linux development setup using Fedora KDE, a Distrobox/Kontainer container, Proton, and WiVRn Flatpak, see
**[docs/FEDORA-KONTAINER-WIVRN.md](docs/FEDORA-KONTAINER-WIVRN.md)**.

### Windows 11: isolated WSL2 build (Ubuntu 24.04)

This is a tested, minimal build workflow for the `erans-build` branch. It compiles the Windows-compatible NOVR assemblies inside Ubuntu under **WSL2**, without installing development SDKs on the Windows host or deploying the build into the live Steam game. The built ZIP can be installed manually as described above.

1. On **Windows 11**, install Ubuntu 24.04 in WSL2 if needed, from PowerShell:

    ```powershell
    wsl --install -d Ubuntu-24.04
    ```

    Reboot if Windows requests it, complete the Ubuntu user setup, then open the Ubuntu terminal. Verify that Ubuntu is using WSL version 2 with `wsl --list --verbose` in PowerShell.

2. **Inside Ubuntu (bash)**, install the build tools and clone this branch:

    ```bash
    sudo apt update
    sudo apt install -y git unzip dotnet-sdk-10.0
    dotnet --version

    mkdir -p ~/Projects
    git clone --branch erans-build --single-branch \
        https://github.com/eran100/novr.git ~/Projects/novr
    cd ~/Projects/novr
    ```

    .NET SDK 10 was used successfully to build the repository's .NET Framework 4.8 plugin projects and .NET 9 build orchestrator. SDK/package availability can differ on other distributions.

3. With **Nuclear Option** and BepInEx 5.x already installed on Windows, locate the Steam game folder. The following is an *example* Windows path; change it to your actual installation:

    ```bash
    GAME_SOURCE="$(wslpath -u 'C:\Program Files (x86)\Steam\steamapps\common\Nuclear Option')"
    GAME_REFS="$HOME/novr-game-refs"

    # Verify the game references before copying.
    ls "$GAME_SOURCE/NuclearOption_Data/Managed/Assembly-CSharp.dll"
    ls "$GAME_SOURCE/BepInEx/core/BepInEx.dll"

    # Copy references into WSL; leave the Windows game untouched.
    mkdir -p "$GAME_REFS/NuclearOption_Data" "$GAME_REFS/BepInEx"
    cp -a "$GAME_SOURCE/NuclearOption_Data/Managed" "$GAME_REFS/NuclearOption_Data/"
    cp -a "$GAME_SOURCE/BepInEx/core" "$GAME_REFS/BepInEx/"
    ```

    Do **not** point `NuclearOptionGameDirResolved` at the live Windows Steam directory. The build targets can automatically deploy output to the game directory specified by that property. A separate WSL reference copy keeps those writes away from the installed game.

4. Restore, build the **release packaging project** (rather than the complete Windows/MSVC solution), and validate the ZIP:

    ```bash
    cd ~/Projects/novr
    dotnet restore NOVR.Build/NOVR.Build.csproj \
        -p:NuclearOptionGameDirResolved="$HOME/novr-game-refs"

    dotnet build NOVR.Build/NOVR.Build.csproj \
        --configuration Release --no-restore \
        -p:NuclearOptionGameDirResolved="$HOME/novr-game-refs"

    ls -lh dist/NOVR.zip
    unzip -t dist/NOVR.zip | tail -5
    unzip -Z1 dist/NOVR.zip | grep -E \
        '^(plugins/NOVR/NOVR.dll|patchers/NOVR/NOVR.Patcher.dll)
The GUI installer is built with Avalonia. Building it in `Release` automatically publishes raw installer binaries for Linux and Windows into `dist/`:

```bash
dotnet build NOVR.Installer/NOVR.Installer.csproj -c Release
```

Outputs:

- `dist/NOVR.Installer-Linux`
- `dist/NOVR.Installer-Win.exe`

Building the full solution in `Release` also creates `dist/NOVR.zip`, ready to upload as the mod release asset.

## License

    Nuclear Option VR Mod
    Copyright (C) 2026 InfernoSuperNova (DeltaWing)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

# Original README below

> # Universal Unity VR
>
> [![Raicuparta's VR mods](https://raicuparta.com/img/badge.svg)](https://raicuparta.com)
>
> Use [Rai Pal](https://pal.raicuparta.com) to install this mod.
>
> ## License
>
>     Rai Pal
>     Copyright (C) 2024  Raicuparta
>
>     This program is free software: you can redistribute it and/or modify
>     it under the terms of the GNU General Public License as published by
>     the Free Software Foundation, either version 3 of the License, or
>     (at your option) any later version.
>
>     This program is distributed in the hope that it will be useful,
>     but WITHOUT ANY WARRANTY; without even the implied warranty of
>     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
>     GNU General Public License for more details.
>
>     You should have received a copy of the GNU General Public License
>     along with this program.  If not, see <https://www.gnu.org/licenses/>.

    ```

    The resulting package is `~/Projects/novr/dist/NOVR.zip`. From Windows File Explorer, browse to `\\wsl.localhost\Ubuntu-24.04\home\<ubuntu-user>\Projects\novr\dist`, copy the ZIP to Windows, and extract its `plugins` and `patchers` folders into `Nuclear Option/BepInEx`. Close the game before copying.

5. Build verification is **not** gameplay verification. Test in VR after installing, particularly the mission picker, single-/multiplayer mission lists, and Steam Workshop browsing. The game may change its APIs in future releases. For example, this branch was updated for a game version exposing `MissionPicker` (singular), `MissionSaveLoad.QuickLoadOne` instead of `QuickLoadMany`, and `ModType.Name` rather than `ModType.Tag`. If compilation breaks after a game update, inspect the installed `Assembly-CSharp.dll` rather than blindly renaming APIs. The `ilspycmd` .NET tool can inspect the copied game assembly:

    ```bash
    dotnet tool install ilspycmd --tool-path "$HOME/.local/share/novr-tools"
    "$HOME/.local/share/novr-tools/ilspycmd" -l c \
        "$HOME/novr-game-refs/NuclearOption_Data/Managed/Assembly-CSharp.dll" \
        | grep -Ei 'MissionPicker|MissionSaveLoad|ModType'
    ```

To rebuild after changing source code, repeat step 4. You do not need to reinstall the SDK, recopy unchanged game assemblies, or reinstall BepInEx; update the WSL reference copies if Nuclear Option itself is updated.

## Installer development

The GUI installer is built with Avalonia. Building it in `Release` automatically publishes raw installer binaries for Linux and Windows into `dist/`:

```bash
dotnet build NOVR.Installer/NOVR.Installer.csproj -c Release
```

Outputs:

- `dist/NOVR.Installer-Linux`
- `dist/NOVR.Installer-Win.exe`

Building the full solution in `Release` also creates `dist/NOVR.zip`, ready to upload as the mod release asset.

## License

    Nuclear Option VR Mod
    Copyright (C) 2026 InfernoSuperNova (DeltaWing)

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.

    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

# Original README below

> # Universal Unity VR
>
> [![Raicuparta's VR mods](https://raicuparta.com/img/badge.svg)](https://raicuparta.com)
>
> Use [Rai Pal](https://pal.raicuparta.com) to install this mod.
>
> ## License
>
>     Rai Pal
>     Copyright (C) 2024  Raicuparta
>
>     This program is free software: you can redistribute it and/or modify
>     it under the terms of the GNU General Public License as published by
>     the Free Software Foundation, either version 3 of the License, or
>     (at your option) any later version.
>
>     This program is distributed in the hope that it will be useful,
>     but WITHOUT ANY WARRANTY; without even the implied warranty of
>     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
>     GNU General Public License for more details.
>
>     You should have received a copy of the GNU General Public License
>     along with this program.  If not, see <https://www.gnu.org/licenses/>.
