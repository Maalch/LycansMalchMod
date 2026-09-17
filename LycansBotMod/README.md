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