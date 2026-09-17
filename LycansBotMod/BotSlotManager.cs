using System.Collections.Generic;
using System.Linq;
using Fusion;
using HarmonyLib;
using UnityEngine;

namespace LycansBotMod;

// Spawns/removes host-local fake PlayerRef slots (no real client, no input authority) for basic structural testing.
internal static class BotSlotManager
{
	private const int FirstBotPlayerId = 1000;

	private static int _nextBotPlayerId = FirstBotPlayerId;

	private static readonly HashSet<int> _botPlayerIds = new HashSet<int>();

	internal static bool IsBot(PlayerRef playerRef)
	{
		return _botPlayerIds.Contains(playerRef.PlayerId);
	}

	internal static void SpawnBot(NetworkRunner runner)
	{
		if (runner == null || !runner.IsServer)
		{
			return;
		}

		Spawner spawner = Object.FindObjectOfType<Spawner>();
		if (spawner == null)
		{
			Plugin.BotLogger.LogWarning((object)"BotSlotManager: no Spawner found in scene.");
			return;
		}

		PlayerController playerControllerPrefab = Traverse.Create(spawner).Field<PlayerController>("_playerControllerPrefab").Value;
		if (playerControllerPrefab == null)
		{
			Plugin.BotLogger.LogWarning((object)"BotSlotManager: Spawner has no player controller prefab assigned.");
			return;
		}

		PlayerRef botRef = _nextBotPlayerId;
		_nextBotPlayerId++;
		_botPlayerIds.Add(botRef.PlayerId);

		GameManager.Instance.OnPlayerJoined(runner, botRef, playerControllerPrefab);
		Plugin.BotLogger.LogInfo((object)("BotSlotManager: spawning bot, ref " + botRef.PlayerId));
	}

	internal static void RemoveLastBot(NetworkRunner runner)
	{
		if (runner == null || !runner.IsServer || _botPlayerIds.Count == 0)
		{
			return;
		}

		RemoveBot(runner, _botPlayerIds.Max());
	}

	private static void RemoveBot(NetworkRunner runner, int botPlayerId)
	{
		if (!_botPlayerIds.Remove(botPlayerId))
		{
			return;
		}

		GameManager.Rpc_DeletePlayer(runner, botPlayerId);
		Plugin.BotLogger.LogInfo((object)("BotSlotManager: removed bot, ref " + botPlayerId));
	}
}