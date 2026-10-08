using System;
using HarmonyLib;
using LycansNewRoles;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class ShowTargetedPlayerRoleDebugKeybindPatch
{
	private static readonly AccessTools.FieldRef<PlayerController, GameObject> GunTargetObject = AccessTools.FieldRefAccess<PlayerController, GameObject>("_gunTargetObject");
	private static string _displayText;
	private static int _displayFrame = -1;
	private static bool _errorLogged;
	private static GUIStyle _displayStyle;

	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (__instance.Object == null || !__instance.Object.HasInputAuthority)
			{
				return;
			}

			_displayText = null;
			UpdateDisplay(__instance);
			_errorLogged = false;
		}
		catch (Exception e)
		{
			_displayText = null;
			if (!_errorLogged)
			{
				Plugin.BotLogger.LogError((object)("ShowTargetedPlayerRoleDebugKeybindPatch error: " + e));
				_errorLogged = true;
			}
		}
	}

	private static void UpdateDisplay(PlayerController localPlayer)
	{
		if (GameManager.LocalGameState != GameState.EGameState.Play || localPlayer.IsDead)
		{
			return;
		}

		GameObject gunTargetObject = GunTargetObject(localPlayer);
		if (gunTargetObject == null)
		{
			return;
		}

		PlayerController targetPlayer = gunTargetObject.GetComponentInParent<PlayerController>();
		if (targetPlayer == null || targetPlayer == localPlayer)
		{
			return;
		}

		PlayerCustom targetCustom = PlayerCustomRegistry.GetPlayer(targetPlayer.Ref);
		_displayFrame = Time.frameCount;
		if ((object)targetCustom == null)
		{
			_displayText = targetPlayer.PlayerData.Username + "\nRole data unavailable.";
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

		_displayText = targetPlayer.PlayerData.Username + "\n" + roleDescription;
	}

	internal static void DrawOverlay()
	{
		if (string.IsNullOrEmpty(_displayText) || _displayFrame != Time.frameCount || GameManager.LocalGameState != GameState.EGameState.Play)
		{
			return;
		}

		if (_displayStyle == null)
		{
			_displayStyle = new GUIStyle(GUI.skin.box)
			{
				alignment = TextAnchor.MiddleCenter,
				fontSize = 18,
				wordWrap = true,
				richText = false,
				padding = new RectOffset(12, 12, 8, 8)
			};
			_displayStyle.normal.textColor = Color.white;
		}

		float width = Mathf.Min(560f, Screen.width - 24f);
		GUIContent content = new GUIContent(_displayText);
		float height = _displayStyle.CalcHeight(content, width);
		float top = Mathf.Min(Screen.height * 0.5f + 36f, Screen.height - height - 12f);
		GUI.Box(new Rect((Screen.width - width) * 0.5f, top, width, height), content, _displayStyle);
	}
}
