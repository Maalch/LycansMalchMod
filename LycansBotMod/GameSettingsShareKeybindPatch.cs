using System;
using HarmonyLib;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(PlayerController), "Update")]
internal class GameSettingsShareKeybindPatch
{
	private static void Postfix(PlayerController __instance)
	{
		try
		{
			if (GameManager.LocalGameState != GameState.EGameState.Pregame)
			{
				return;
			}

			if (!__instance.Object.HasInputAuthority || !__instance.Runner.IsServer)
			{
				return;
			}

			if (Input.GetKeyDown(KeyCode.F9))
			{
				CopySettingsToClipboard();
			}
			else if (Input.GetKeyDown(KeyCode.F10))
			{
				ApplySettingsFromClipboard();
			}
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("GameSettingsShareKeybindPatch error: " + e));
		}
	}

	// Set right before DisplayMessage so the CloseMessage patch below only re-locks the cursor for our own popups.
	private static bool _cursorRestorePending;

	private static void CopySettingsToClipboard()
	{
		GameSettingsUI gameSettingsUI = UnityEngine.Object.FindObjectOfType<GameSettingsUI>(true);
		string code = GameSettingsShareManager.ExportCode(gameSettingsUI);
		GUIUtility.systemCopyBuffer = code;
		ShowMessageWithCursor("Game settings code copied to clipboard.");
		Plugin.BotLogger.LogInfo((object)"GameSettingsShareKeybindPatch: settings code copied to clipboard.");
	}

	private static void ApplySettingsFromClipboard()
	{
		GameSettingsUI gameSettingsUI = UnityEngine.Object.FindObjectOfType<GameSettingsUI>(true);
		string code = GUIUtility.systemCopyBuffer;
		if (GameSettingsShareManager.TryApplyCode(gameSettingsUI, code, out int appliedCount, out int skippedCount, out string error))
		{
			ShowMessageWithCursor($"Applied {appliedCount} game settings from clipboard.");
			Plugin.BotLogger.LogInfo((object)$"GameSettingsShareKeybindPatch: applied {appliedCount} settings, skipped {skippedCount}.");
		}
		else
		{
			ShowMessageWithCursor("Could not apply settings code: " + error);
			Plugin.BotLogger.LogWarning((object)("GameSettingsShareKeybindPatch: failed to apply clipboard code: " + error));
		}
	}

	private static void ShowMessageWithCursor(string message)
	{
		GameManager.Instance.gameUI.UpdateCursor(true);
		_cursorRestorePending = true;
		GameManager.Instance.gameUI.DisplayMessage(message);
	}

	internal static bool ConsumeCursorRestorePending()
	{
		bool pending = _cursorRestorePending;
		_cursorRestorePending = false;
		return pending;
	}
}

// The message popup shown by GameUI.DisplayMessage has no cursor of its own during gameplay; re-lock it once the popup is closed.
[HarmonyPatch(typeof(GameUI), "CloseMessage")]
internal class GameSettingsShareMessageClosedPatch
{
	private static void Postfix()
	{
		try
		{
			if (!GameSettingsShareKeybindPatch.ConsumeCursorRestorePending())
			{
				return;
			}
			GameManager.Instance.gameUI.UpdateCursor(false);
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("GameSettingsShareMessageClosedPatch error: " + e));
		}
	}
}
