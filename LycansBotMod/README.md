# Lycans Bot Mod Testing

## Bot slots vs. a second client

`BotSlotManager` adds host-local bot slots by handing `GameManager.Instance.OnPlayerJoined` a synthetic `PlayerRef` (starting at 1000) instead of one issued by a connecting client. Because the host spawns the resulting `PlayerController`, it gets normal state authority, so it registers in `PlayerRegistry` and gets a `PlayerCustom` entry the same way a real player would (see `AddPlayerAddCustomPlayerPatch` in `LycansNewRoles`).

Bots never have **input authority** (no real client owns the connection), so they never move, act, or vote on their own, and `PlayerController.AfterSpawned`'s normal username/hat RPC path is skipped; `BotSlotAfterSpawnedPatch` fills that in directly for bot ids. Use bots for structural/UI tests that only need extra player *slots* to exist (player counts, meeting/target lists, kill/death handling aimed at a bot, win-condition checks).

Use a real second client for anything that requires the other side to act: voting, role/meeting flows driven by the second player, or effects that depend on real input.

## Prerequisites

- A Lycans game installation with BepInEx installed.
- Permission from the game and its platform/authentication flow to run two clients on the same Windows PC.
- Any separate local sessions or accounts required by Lycans' normal multiplayer flow.

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

## Two-client workflow

1. Build and deploy the mod.
2. Start the first Lycans client using a supported local launch method.
3. Host a multiplayer session through Lycans' normal in-game multiplayer UI.
4. Start a second Lycans client using a supported local launch method.
5. Join the hosted session from the second client through Lycans' normal in-game join/session UI.
6. Confirm that both players have entered the session before starting the test.

This repository does not expose a supported command-line API for hosting or joining a Lycans session. Keep hosting and joining in the native game UI.

## Readiness checks

Before a multiplayer behavior test, verify all of the following:

- Both clients are visible as separate players in the game.
- The host log contains `Lycans Bot Mod loaded.`
- Both participants are available in meeting and target-selection UI.

> Running two local clients at once may hit a sharing violation on `HarmonyLog.txt` (LycansNewRoles enables HarmonyFileLog). The `LycansHarmonyLogPatcher` project that worked around this has been removed for now; reintroduce it if two-client testing needs it again.

When a real client joins, the normal game lifecycle gives it a Fusion `PlayerRef`, creates its `PlayerController`, and registers its custom data. This is the required state for valid multiplayer tests.

## Test recipes

With both clients connected, use normal gameplay to test:

- Proceeding past the first meeting with more than one participant.
- Applying effects from one real player to the other.
- Kill and death behavior, including the normal callback and role handling paths.
- Votes, meeting behavior, role interactions, and victory conditions that enumerate active players.

If two game clients cannot run on the same PC, host on one machine and join with a real client on a second machine.

## Bot controls

As the host, during Pregame:

- `Keypad +` spawns a bot slot (`BotSlotKeybindPatch` -> `BotSlotManager.SpawnBot`).
- `Keypad -` removes the most recently added bot slot (`BotSlotManager.RemoveLastBot`, via `GameManager.Rpc_DeletePlayer`).

See `BotSlotManager.cs`, `BotSlotKeybindPatch.cs`, and `BotSlotAfterSpawnedPatch.cs` for the implementation.