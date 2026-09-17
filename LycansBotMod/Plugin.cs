using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace LycansBotMod;

[BepInPlugin("LycansBotMod", "Lycans Bot Mod", "0.1.0")]
[BepInDependency("LycansNewRoles")]
public class Plugin : BaseUnityPlugin
{
	internal static ManualLogSource BotLogger;

	private Harmony harmony;

	private void Awake()
	{
		BotLogger = Logger;
		harmony = new Harmony("lycans.botmod");
		harmony.PatchAll();
		BotLogger.LogInfo((object)"Lycans Bot Mod loaded.");
		BotLogger.LogInfo((object)"Host: Pregame, press Keypad+ / Keypad- to add/remove a bot slot.");
	}

	private void OnDestroy()
	{
		harmony?.UnpatchSelf();
	}
}