# Lycans Bot Mod Testing

## Bot slots

`BotSlotManager` adds host-local bot slots by handing `GameManager.Instance.OnPlayerJoined` a synthetic `PlayerRef` (starting at 1000) instead of one issued by a connecting client. Because the host spawns the resulting `PlayerController`, it gets normal state authority, so it registers in `PlayerRegistry` and gets a `PlayerCustom` entry the same way a real player would (see `AddPlayerAddCustomPlayerPatch` in `LycansNewRoles`).

Bots never have **input authority** (no real client owns the connection), so they never move, act, or vote on their own, and `PlayerController.AfterSpawned`'s normal username/hat RPC path is skipped; `BotSlotAfterSpawnedPatch` fills that in directly for bot ids. Use bots for structural/UI tests that only need extra player *slots* to exist (player counts, meeting/target lists, kill/death handling aimed at a bot, win-condition checks).

Bots can't vote, act, or otherwise respond, so anything requiring real input from another participant (voting outcomes, role/meeting flows, effects driven by another player) isn't testable with bots alone.

## Prerequisites

- A Lycans game installation with BepInEx installed.

If your game installation is not at the default Steam location, set `LycansGameDir` when building. The build derives the managed assemblies, BepInEx core, and plugin locations from that property.

## Build and deploy

Build and deploy the Bot Mod from `LycansMalchMod\LycansBotMod`:

```powershell
dotnet build LycansBotMod.csproj -t:Rebuild -p:DeployToLycans=true
```

The resulting `LycansBotMod.dll` is deployed to:

```text
<LycansGameDir>\BepInEx\plugins\LycansBotMod
```

For a non-default installation, provide the game path explicitly:

```powershell
dotnet build LycansBotMod.csproj -t:Rebuild -p:DeployToLycans=true -p:LycansGameDir="D:\Games\Lycans"
```

## Bot controls

As the host, during Pregame:

- `Keypad +` spawns a bot slot (`BotSlotKeybindPatch` -> `BotSlotManager.SpawnBot`).
- `Keypad -` removes the most recently added bot slot (`BotSlotManager.RemoveLastBot`, via `GameManager.Rpc_DeletePlayer`).

See `BotSlotManager.cs`, `BotSlotKeybindPatch.cs`, and `BotSlotAfterSpawnedPatch.cs` for the implementation.

## Sharing game settings

As the host, during Pregame:

- `F9` copies a share code for the current `GameConfig` settings (roles, powers, events, potions, gadgets, accessories, and the other dropdowns/toggles) to the clipboard.
- `F10` applies a share code from the clipboard, so another host can reproduce the same configuration.
- The Game Settings screen also has a "Share Settings" button with the same Copy/Apply actions plus a visible code for pasting into chat.

The code is a version-tagged, base64-encoded snapshot of `GameConfig`'s public dropdown/toggle fields; applying it only updates those live UI controls, so `GameConfig`'s existing listeners handle persisting the values normally. See `GameSettingsShareManager.cs`, `GameSettingsShareKeybindPatch.cs`, and `GameSettingsSharePanel.cs`.

## Self-kill debug key

As the host, during Play:

- `F4` kills your own player, recording the death as `STARVATION` (`SelfKillDebugKeybindPatch` -> `PlayerController.Rpc_Kill`).
- `F3` kills the player you're currently aiming at (`PlayerController._gunTargetObject`, a role-agnostic aim raycast — unlike `targetObject`, which is gated by role-specific interact permissions), recording the death as `BULLET_HUMAN` (`KillTargetedPlayerDebugKeybindPatch` -> `PlayerController.Rpc_Kill`).

See `SelfKillDebugKeybindPatch.cs` and `KillTargetedPlayerDebugKeybindPatch.cs` for the implementation.