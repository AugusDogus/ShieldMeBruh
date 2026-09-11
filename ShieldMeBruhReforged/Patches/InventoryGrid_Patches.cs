using HarmonyLib;

namespace ShieldMeBruhReforged.Patches;

public static class InventoryGrid_Patches
{
    [HarmonyPatch(typeof(InventoryGrid), nameof(InventoryGrid.UpdateGui))]
    public static class InventoryGridUpdateGuiPatch
    {
        private static bool _initializedElement;

        public static void ResetInitializedElement()
        {
            _initializedElement = false;
        }
        
        [HarmonyPriority(Priority.First)]
        private static void Prefix(InventoryGrid __instance, ref bool __state)
        {
            if (!__instance.name.Equals("PlayerGrid"))
                return;

            __state = false;

            var width = __instance.m_inventory.GetWidth();
            var height = __instance.m_inventory.GetHeight();

            if (__instance.m_width != width || __instance.m_height != height)
            {
                ShieldMeBruhReforged.Log.LogDebug($"Width {width} doesn't match {__instance.m_width}");
                ShieldMeBruhReforged.Log.LogDebug($"Height {height} doesn't match {__instance.m_height}");
                __state = true;
                _initializedElement = false;
            }
        }

        private static void Postfix(InventoryGrid __instance, ref bool __state, bool __runOriginal)
        {
            if (!__instance.name.Equals("PlayerGrid"))
                return;

            if (!__state || !__runOriginal)
                return;

            ShieldMeBruhReforged.Log.LogDebug("Inventory Grid needs to init.");
            
            foreach (var element in __instance.m_elements)
            {
                var gameObject = element.gameObject;
                var inputHandler = gameObject.GetComponentInChildren<UIInputHandler>();
                inputHandler.m_onMiddleDown += ShieldMeBruhReforged.AutoShield.OnMiddleClick;
                ShieldMeBruhReforged.Log.LogDebug($"Adding to element: X: {element.Position.x}  Y: {element.Position.y}");
            }

            if (!_initializedElement && Player.m_localPlayer.m_customData.ContainsKey(ShieldMeBruhReforged.PluginId))
            {
                
                if (ShieldMeBruhReforged.AutoShield.GetSavedShieldPosition() is { } savedElementVector)
                {
                    var savedElement =
                        __instance.GetElement(savedElementVector.x, savedElementVector.y, __instance.m_width);
                    var savedItem = __instance.m_inventory.GetItemAt(savedElementVector.x, savedElementVector.y);

                    if (savedElement != null && savedItem != null) ShieldMeBruhReforged.AutoShield.ApplyShieldToElement(savedElement, savedItem);
                }

                _initializedElement = true;
            }
        }
    }
}
