using Fusion;
using HarmonyLib;
using UnityEngine;

namespace LycansBotMod;

[HarmonyPatch(typeof(CharacterMovementHandler), "FixedUpdateNetwork")]
internal class BotSlotTeleportDataPatch
{
	private static void Postfix(CharacterMovementHandler __instance)
	{
		if (!((SimulationBehaviour)__instance).HasStateAuthority)
		{
			return;
		}

		PlayerController playerController = Traverse.Create((object)__instance).Field<PlayerController>("_playerController").Value;
		if (playerController == null || !BotSlotManager.IsBot(playerController.Ref))
		{
			return;
		}

		NetworkTeleportData teleportData = __instance.TeleportData;
		if (((NetworkTeleportData)(ref teleportData)).IsNone)
		{
			return;
		}

		Transform playerTransform = ((Component)playerController).transform;
		playerTransform.position = teleportData.Position;
		playerTransform.rotation = teleportData.Rotation;
		if (teleportData.ResetLook)
		{
			playerController.Rpc_UpdateRotation();
		}

		__instance.TeleportData = NetworkTeleportData.None;
	}
}
