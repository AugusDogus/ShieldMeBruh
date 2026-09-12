using HarmonyLib;

namespace ShieldMeBruhReforged.Patches;

public static class InventoryGrid_Patches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    private static class InventoryGridUpdateGuiPatch
    {
        [HarmonyPriority(Priority.First)]
        private static void Prefix(InventoryGrid __instance, ref bool __state)
        {
            if (Player.m_localPlayer is not { } player || __instance.m_inventory != player.GetInventory()) return;
            __state = __instance.m_width != __instance.m_inventory.GetWidth() ||
                __instance.m_height != __instance.m_inventory.GetHeight();
        }

        private static void Postfix(InventoryGrid __instance, bool __state, bool __runOriginal)
        {
            if (!__runOriginal || Player.m_localPlayer is not { } player ||
                __instance.m_inventory != player.GetInventory()) return;

            if (__state)
            {
                foreach (var element in __instance.m_elements)
                {
                    var input = element.gameObject.GetComponentInChildren<UIInputHandler>();
                    input.m_onMiddleDown += ShieldMeBruhReforged.AutoShield.OnMiddleClick;
                }
            }

            ShieldMeBruhReforged.AutoShield.RefreshSelection(__instance);
        }
    }
}
