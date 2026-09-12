using HarmonyLib;

namespace ShieldMeBruhReforged.Patches;

public static class Player_Patches
{
    [HarmonyPatch(typeof(Player), nameof(Player.SetLocalPlayer))]
    private static class SetLocalPlayerPatch
    {
        static void Postfix(Player __instance)
        {
            if (__instance == Player.m_localPlayer)
                ShieldMeBruhReforged.AutoShield.ResetPlayerContext();
        }
    }

}
