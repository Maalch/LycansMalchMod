using System;
using HarmonyLib;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class BotSlotKeybindPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (GameManager.LocalGameState != GameState.EGameState.Pregame)
			{
				return;
			}

			if (!__instance.Object.HasInputAuthority)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.KeypadPlus))
			{
				BotSlotManager.SpawnBot(__instance.Runner);
			}
			else if (Input.GetKeyDown(KeyCode.KeypadMinus))
			{
				BotSlotManager.RemoveLastBot(__instance.Runner);
			}
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("BotSlotKeybindPatch error: " + e));
		}
	}
}
