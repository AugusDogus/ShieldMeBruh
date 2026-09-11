using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using ShieldMeBruhReforged.Features;

namespace ShieldMeBruhReforged;

[BepInPlugin(PluginId, "Shield Me Bruh Reforged", "1.0.0")]
[BepInDependency("com.ValheimModding.YamlDotNetDetector")]
public class ShieldMeBruhReforged : BaseUnityPlugin
{
    public const string PluginId = "augusdogus.mods.shieldmebruhreforged";

    private Harmony _harmony;

    public static ManualLogSource Log { get; private set; }
    public static AutoShield AutoShield { get; private set; }

    private void Awake()
    {
        Log = Logger;
        AutoShield = new AutoShield(Config);
        AutoShield.LoadAssets();
        _harmony = new Harmony(Info.Metadata.GUID);
        _harmony.PatchAll(Assembly.GetExecutingAssembly());
    }

    private void OnDestroy()
    {
        _harmony?.UnpatchSelf();
        AutoShield?.Dispose();
        AutoShield = null;
    }
}
