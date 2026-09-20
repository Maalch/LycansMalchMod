using System;
using Fusion;
using HarmonyLib;
using LycansNewRoles;
using LycansNewRoles.Stats;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class SelfKillDebugKeybindPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (GameManager.LocalGameState != GameState.EGameState.Play)
			{
				return;
			}

			if (!__instance.Object.HasInputAuthority || !__instance.Runner.IsServer)
			{
				// Death type tracking and Rpc_Kill require host/state authority; non-host presses are ignored.
				return;
			}

			if (!Input.GetKeyDown(KeyCode.F4))
			{
				return;
			}

			if (__instance.IsDead)
			{
				return;
			}

			PlayerCustomRegistry.GetPlayer(__instance.Ref).Stats.UpdateDeathType(PlayerStats.DeathTypeStarvation);
			__instance.Rpc_Kill(PlayerRef.None);
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("SelfKillDebugKeybindPatch error: " + e));
		}
	}
}
