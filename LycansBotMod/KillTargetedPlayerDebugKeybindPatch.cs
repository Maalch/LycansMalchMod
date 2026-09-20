using System;
using Fusion;
using HarmonyLib;
using LycansNewRoles;
using LycansNewRoles.Stats;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class KillTargetedPlayerDebugKeybindPatch
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

			if (!Input.GetKeyDown(KeyCode.F3))
			{
				return;
			}

			if ((object)__instance.targetObject == null)
			{
				return;
			}

			PlayerController targetPlayer = __instance.targetObject.GetComponentInParent<PlayerController>();
			if ((object)targetPlayer == null || targetPlayer.Ref == __instance.Ref || targetPlayer.IsDead)
			{
				return;
			}

			PlayerCustomRegistry.GetPlayer(targetPlayer.Ref).Stats.UpdateDeathType(PlayerStats.DeathTypeBulletHumanForm);
			targetPlayer.Rpc_Kill(__instance.Ref);
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("KillTargetedPlayerDebugKeybindPatch error: " + e));
		}
	}
}
