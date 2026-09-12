using HarmonyLib;

namespace ShieldMeBruhReforged.Patches;

public static class Humanoid_Patches
{
    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.EquipItem))]
    private static class HumanoidEquipItemPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item, bool __result,
            ItemDrop.ItemData ___m_leftItem, bool __runOriginal)
        {
            if (__instance is not Player player || player != Player.m_localPlayer || !__runOriginal || !__result ||
                !ShieldMeBruhReforged.AutoShield.EnableAutoShield.Value || item == null ||
                item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon || ___m_leftItem != null)
                return;

            var shield = ShieldMeBruhReforged.AutoShield.GetSelectedShield(player);
            if (shield != null) player.EquipItem(shield);
        }
    }

    [HarmonyPatch(typeof(Humanoid), nameof(Humanoid.UnequipItem))]
    private static class HumanoidUnequipItemPatch
    {
        private static void Postfix(Humanoid __instance, ItemDrop.ItemData item,
            ItemDrop.ItemData ___m_leftItem, bool __runOriginal)
        {
            if (__instance is not Player player || player != Player.m_localPlayer || !__runOriginal ||
                !ShieldMeBruhReforged.AutoShield.EnableAutoShield.Value ||
                !ShieldMeBruhReforged.AutoShield.EnableAutoUnequip.Value || item == null || ___m_leftItem == null ||
                item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.OneHandedWeapon)
                return;

            if (___m_leftItem == ShieldMeBruhReforged.AutoShield.GetSelectedShield(player))
                player.UnequipItem(___m_leftItem);
        }
    }
}
