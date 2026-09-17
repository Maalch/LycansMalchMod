using System;
using System.Collections.Generic;
using System.Linq;
using Fusion;
using HarmonyLib;
using LycansNewRoles;
using LycansNewRoles.NewItems;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class DebugItemSpawnKeybindPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (GameManager.LocalGameState != GameState.EGameState.Play)
			{
				return;
			}

			if (!__instance.Object.HasInputAuthority)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.F1))
			{
				SpawnScrollNextToPlayer(__instance);
			}
			else if (Input.GetKeyDown(KeyCode.F2))
			{
				SpawnPotionNextToPlayer(__instance);
			}
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("DebugItemSpawnKeybindPatch error: " + e));
		}
	}

	private static void SpawnScrollNextToPlayer(PlayerController player)
	{
		NetworkRunner runner = player.Runner;
		if (runner == null || !runner.IsServer)
		{
			// Item spawning requires host/state authority; non-host presses are ignored.
			return;
		}

		Item prefab = Traverse.Create((object)GameManager.Instance).Field<Item[]>("spawnableItemPrefabs").Value
			.FirstOrDefault(item => item is MagicScrollItem);
		if (prefab == null)
		{
			Plugin.BotLogger.LogWarning((object)"DebugItemSpawnKeybindPatch: no MagicScrollItem prefab found.");
			return;
		}

		Vector3 position = player.transform.position + player.transform.forward;
		ItemUtility.SpawnItem(prefab, position, Quaternion.identity, runner);
	}

	private static void SpawnPotionNextToPlayer(PlayerController player)
	{
		NetworkRunner runner = player.Runner;
		if (runner == null || !runner.IsServer)
		{
			// Item spawning requires host/state authority; non-host presses are ignored.
			return;
		}

		Potion potionPrefab = Traverse.Create((object)GameManager.Instance).Field<Potion>("potionPrefab").Value;
		List<Effect> potionEffects = Traverse.Create((object)GameManager.Instance).Field<List<Effect>>("_potionEffects").Value;
		if (potionPrefab == null || potionEffects.Count == 0)
		{
			Plugin.BotLogger.LogWarning((object)"DebugItemSpawnKeybindPatch: no potion effects available.");
			return;
		}

		Effect effect = potionEffects[UnityEngine.Random.Range(0, potionEffects.Count)];
		int localEffectIndex = potionEffects.IndexOf(effect);
		int globalEffectIndex = EffectManager.GetEffectIndex(effect);
		Vector3 position = player.transform.position + player.transform.forward;
		runner.Spawn<Potion>(potionPrefab, position, Quaternion.identity, onBeforeSpawned: (NetworkRunner.OnBeforeSpawned)delegate(NetworkRunner _, NetworkObject no)
		{
			no.GetComponent<Potion>().Init(localEffectIndex, globalEffectIndex);
		});
	}
}
