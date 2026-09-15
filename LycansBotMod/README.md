# Lycans Bot Mod Testing

## Why a second client is required

`LycansBotMod` cannot create a phantom player slot. In this Fusion build, a valid `PlayerRef` is issued only when a client connects to a game session. The game then creates the player's `PlayerController` and registers its matching `PlayerCustom` entry.

Do not create a fake `PlayerRef`, add an entry directly to `PlayerCustomRegistry`, or spawn a player controller with `PlayerRef.None`. Those objects do not have a connected client's authority or the lifecycle required by the game, player registry, and UI.

Use a real second client for tests that need another player: meetings, voting, role assignment, target-dependent effects, kills, death handling, and win conditions.

## Prerequisites

- A Lycans game installation with BepInEx installed.
- Permission from the game and its platform/authentication flow to run two clients on the same Windows PC.
- Any separate local sessions or accounts required by Lycans' normal multiplayer flow.

If your game installation is not at the default Steam location, set `LycansGameDir` when building. The build derives the managed assemblies, BepInEx core, and plugin locations from that property.

## Build and deploy

Close every running Lycans client, then build and deploy the Harmony log patcher from `LycansMalchMod\LycansHarmonyLogPatcher`:

```powershell
dotnet build LycansHarmonyLogPatcher.csproj -t:Rebuild -p:DeployToLycans=true
```

This installs `LycansHarmonyLogPatcher.dll` in `<LycansGameDir>\BepInEx\patchers`. It runs before BepInEx loads `LycansNewRoles` and sends Harmony debug output to `HarmonyLog.<process-id>.txt`, allowing each local game client to write independently.

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
- Each client has its own `HarmonyLog.<process-id>.txt` file in the game directory; no client reports a sharing violation for `HarmonyLog.txt`.
- The host log contains the expected warning that real-slot bots are disabled. This confirms the mod is not attempting an invalid synthetic-player path.
- Both participants are available in meeting and target-selection UI.

When a real client joins, the normal game lifecycle gives it a Fusion `PlayerRef`, creates its `PlayerController`, and registers its custom data. This is the required state for valid multiplayer tests.

## Test recipes

With both clients connected, use normal gameplay to test:

- Proceeding past the first meeting with more than one participant.
- Applying effects from one real player to the other.
- Kill and death behavior, including the normal callback and role handling paths.
- Votes, meeting behavior, role interactions, and victory conditions that enumerate active players.

If two game clients cannot run on the same PC, host on one machine and join with a real client on a second machine. Do not substitute a fabricated player slot.