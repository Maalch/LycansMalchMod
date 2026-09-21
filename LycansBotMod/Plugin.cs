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
		BotLogger.LogInfo((object)"Host: Play, press F1 to spawn a scroll, F2 to spawn a potion.");
		BotLogger.LogInfo((object)"Host: Play, press F3 to kill your currently targeted player (recorded as BULLET_HUMAN).");
		BotLogger.LogInfo((object)"Host: Play, press F4 to kill yourself (recorded as STARVATION).");
		BotLogger.LogInfo((object)"Play, press F5 to log the role of your currently targeted player.");
		BotLogger.LogInfo((object)"LycansUtility.AddLogOnlyForMe is patched to always log, regardless of Steam ID.");

	}

	private void OnDestroy()
	{
		harmony?.UnpatchSelf();
	}
}