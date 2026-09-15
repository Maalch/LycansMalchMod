namespace LycansBotMod;

internal static class BotSlotManager
{
	private const string UnsupportedReason = "Real-slot bots are disabled: this Fusion build cannot create a PlayerRef without a connected client.";

	internal static void LogUnsupported()
	{
		Plugin.BotLogger.LogWarning((object)UnsupportedReason);
	}
}