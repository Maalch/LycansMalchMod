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
			if (!__instance.Object.HasInputAuthority)
			{
				return;
			}

			if (!Input.GetKeyDown(KeyCode.F3))
			{
				return;
			}

			if (GameManager.LocalGameState != GameState.EGameState.Play)
			{
				Plugin.BotLogger.LogInfo((object)("KillTargetedPlayerDebugKeybindPatch: ignored, LocalGameState is " + GameManager.LocalGameState + " (needs Play)."));
				return;
			}

			if (!__instance.Runner.IsServer)
			{
				// Death type tracking and Rpc_Kill require host/state authority; non-host presses are ignored.
				Plugin.BotLogger.LogInfo((object)"KillTargetedPlayerDebugKeybindPatch: ignored, local player is not the host.");
				return;
			}

			// targetObject (_targetObject) is gated by CheckPlayerRayCast's role-specific interact permissions (wolf-kill/vote-only),
			// so it stays null for roles with no player-targeting power (e.g. Purificateur). _gunTargetObject is a plain aim raycast
			// with no role gating, so it works regardless of role.
			GameObject gunTargetObject = Traverse.Create((object)__instance).Field<GameObject>("_gunTargetObject").Value;
			if ((object)gunTargetObject == null)
			{
				Plugin.BotLogger.LogInfo((object)"KillTargetedPlayerDebugKeybindPatch: ignored, not aiming at anything (no gun target).");
				return;
			}

			PlayerController targetPlayer = gunTargetObject.GetComponentInParent<PlayerController>();
			if ((object)targetPlayer == null)
			{
				Plugin.BotLogger.LogInfo((object)("KillTargetedPlayerDebugKeybindPatch: ignored, gun target '" + gunTargetObject.name + "' has no PlayerController in its parents."));
				return;
			}

			if (targetPlayer.Ref == __instance.Ref)
			{
				Plugin.BotLogger.LogInfo((object)"KillTargetedPlayerDebugKeybindPatch: ignored, targeted player is yourself.");
				return;
			}

			if (targetPlayer.IsDead)
			{
				Plugin.BotLogger.LogInfo((object)"KillTargetedPlayerDebugKeybindPatch: ignored, targeted player is already dead.");
				return;
			}

			Plugin.BotLogger.LogInfo((object)("KillTargetedPlayerDebugKeybindPatch: killing targeted player " + targetPlayer.PlayerData.Username + "."));
			PlayerCustomRegistry.GetPlayer(targetPlayer.Ref).Stats.UpdateDeathType(PlayerStats.DeathTypeBulletHumanForm);
			targetPlayer.Rpc_Kill(__instance.Ref);
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("KillTargetedPlayerDebugKeybindPatch error: " + e));
		}
	}
}
