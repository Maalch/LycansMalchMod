using System;
using HarmonyLib;
using LycansNewRoles;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class ShowTargetedPlayerRoleDebugKeybindPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (!__instance.Object.HasInputAuthority)
			{
				return;
			}

			if (!Input.GetKeyDown(KeyCode.F5))
			{
				return;
			}

			if (GameManager.LocalGameState != GameState.EGameState.Play)
			{
				Plugin.BotLogger.LogInfo((object)("ShowTargetedPlayerRoleDebugKeybindPatch: ignored, LocalGameState is " + GameManager.LocalGameState + " (needs Play)."));
				return;
			}

			// Same role-agnostic aim raycast used by KillTargetedPlayerDebugKeybindPatch (F3).
			GameObject gunTargetObject = Traverse.Create((object)__instance).Field<GameObject>("_gunTargetObject").Value;
			if ((object)gunTargetObject == null)
			{
				Plugin.BotLogger.LogInfo((object)"ShowTargetedPlayerRoleDebugKeybindPatch: ignored, not aiming at anything (no gun target).");
				return;
			}

			PlayerController targetPlayer = gunTargetObject.GetComponentInParent<PlayerController>();
			if ((object)targetPlayer == null)
			{
				Plugin.BotLogger.LogInfo((object)("ShowTargetedPlayerRoleDebugKeybindPatch: ignored, gun target '" + gunTargetObject.name + "' has no PlayerController in its parents."));
				return;
			}

			PlayerCustom targetCustom = PlayerCustomRegistry.GetPlayer(targetPlayer.Ref);
			if ((object)targetCustom == null)
			{
				Plugin.BotLogger.LogInfo((object)"ShowTargetedPlayerRoleDebugKeybindPatch: ignored, targeted player has no PlayerCustom data.");
				return;
			}

			string roleDescription = targetPlayer.Role + " / " + PlayerCustom.GetNewPrimaryRoleString(targetCustom);
			if (targetCustom.PrimaryRolePower != PlayerCustom.PlayerPrimaryRolePower.None)
			{
				roleDescription += " / " + PlayerCustom.GetPrimaryRolePowerString(targetCustom.PrimaryRolePower);
			}
			if (targetCustom.SecondaryRole != PlayerCustom.PlayerSecondaryRole.None)
			{
				roleDescription += " / " + PlayerCustom.GetSecondaryRoleString(targetCustom.SecondaryRole);
			}

			Plugin.BotLogger.LogInfo((object)("ShowTargetedPlayerRoleDebugKeybindPatch: " + targetPlayer.PlayerData.Username + " is " + roleDescription));
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("ShowTargetedPlayerRoleDebugKeybindPatch error: " + e));
		}
	}
}
