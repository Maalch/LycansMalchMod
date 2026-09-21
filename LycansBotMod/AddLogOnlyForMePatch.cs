using System;
using HarmonyLib;
using LycansNewRoles;

namespace LycansBotMod;

// LycansUtility.AddLogOnlyForMe only logs for a hardcoded Steam ID; force it to always log instead.
[HarmonyPatch(typeof(LycansUtility), "AddLogOnlyForMe")]
internal class AddLogOnlyForMePatch
{
	private static bool Prefix(string log)
	{
		try
		{
			Plugin.BotLogger.LogInfo((object)log);
		}
		catch (Exception e)
		{
			Plugin.BotLogger.LogError((object)("AddLogOnlyForMePatch error: " + e));
		}

		return false;
	}
}
