using System;
using HarmonyLib;
using UnityEngine;

namespace LycansBotMod;

// Bots never gain input authority, so PlayerController.AfterSpawned skips the username/hat setup that real clients get via RPC.
[HarmonyPatch(typeof(PlayerController), "AfterSpawned")]
internal class BotSlotAfterSpawnedPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (!__instance.Object.HasStateAuthority || !BotSlotManager.IsBot(__instance.Ref))
			{
				return;
			}

			NetworkPlayerData playerData = new NetworkPlayerData(Guid.NewGuid().ToString(), "Bot " + __instance.Ref.PlayerId);
			Traverse.Create(__instance).Property("PlayerData", (object[])null).SetValue(playerData);

			int hatCount = __instance.hats.transform.childCount;
			if (hatCount > 0)
			{
				Traverse.Create(__instance).Property("HatIndex", (object[])null).SetValue(UnityEngine.Random.Range(0, hatCount));
			}
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("BotSlotAfterSpawnedPatch error: " + e));
		}
	}
}
