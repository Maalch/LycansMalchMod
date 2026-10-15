using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LycansGameSettingShare;

[BepInPlugin("LycansGameSettingShare", "Lycans Game Setting Share", "0.1.0")]
[BepInDependency("LycansNewRoles")]
public class Plugin : BaseUnityPlugin
{
	internal static ManualLogSource ShareLogger;

	private Harmony harmony;

	private void Awake()
	{
		ShareLogger = Logger;
		harmony = new Harmony("lycans.game-settings-share");
		harmony.PatchAll();
		ShareLogger.LogInfo((object)"Lycans Game Setting Share loaded.");
		ShareLogger.LogInfo((object)"Host: Pregame, press F9 to copy game settings and F10 to apply a settings code.");
	}

	private void OnDestroy()
	{
		harmony?.UnpatchSelf();
	}
}
