# Lycans Game Setting Share

This plugin lets a host copy the current game settings to a share code and apply a code from the clipboard. The Game Settings screen also provides a Share Settings panel.

## Prerequisites

- A Lycans game installation with BepInEx installed.
- LycansNewRoles installed, which provides the `GameConfig` settings that are shared.

If your game installation is not at the default Steam location, set `LycansGameDir` when building. The build derives the managed assemblies, BepInEx core, and plugin locations from that property.

## Build and deploy

Build and deploy the plugin from `LycansMalchMod\LycansGameSettingShare`:

```powershell
dotnet build LycansGameSettingShare.csproj -t:Rebuild -p:DeployToLycans=true
```

The resulting `LycansGameSettingShare.dll` is deployed to:

```text
<LycansGameDir>\BepInEx\plugins\LycansGameSettingShare
```

For a non-default installation, provide the game path explicitly:

```powershell
dotnet build LycansGameSettingShare.csproj -t:Rebuild -p:DeployToLycans=true -p:LycansGameDir="D:\Games\Lycans"
```

## Sharing game settings

As the host, during Pregame:

- `F9` copies a share code for the current `GameConfig` settings to the clipboard.
- `F10` applies a share code from the clipboard.
- The Game Settings screen has a "Share Settings" button with the same Copy/Apply actions and a visible code for pasting into chat.

The code is a version-tagged, base64-encoded snapshot of `GameConfig`'s public dropdown/toggle fields. Applying it updates those live UI controls, so `GameConfig`'s existing listeners handle persisting the values normally.
