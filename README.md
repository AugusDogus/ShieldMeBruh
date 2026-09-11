<p align="center">
  <img src="banner.png" alt="Shield Me Bruh Reforged: auto-equip shields for Valheim" width="900">
</p>

An independent fork of [Shield Me Bruh! by Vapok](https://github.com/Vapok/ShieldMeBruh), maintained by AugusDogus for Valheim 1.0.12. Automatically equips a selected shield when a one-handed weapon is equipped.

Source: [AugusDogus/ShieldMeBruhReforged](https://github.com/AugusDogus/ShieldMeBruhReforged) (private repository).

To set the desired shield, open the Player Inventory and **middle-click** the desired shield to auto-equip.  Press the **middle-mouse** button again to _deselect_ the shield.

This places a shield marker on the selected item. The selection remains saved after logout unless you deselect it.

From there, simply equip a one-handed weapon to automatically equip your shield.

### Valheim 1.0.12 update

Version 1.0.0 targets Valheim **1.0.12** only. Requires **BepInExPack_Valheim 5.4.2350** and **ValheimModding-YamlDotNet 16.3.1**. Jotunn is not required.

Reforged uses plugin ID and character save-data key `augusdogus.mods.shieldmebruhreforged`, with configuration at `BepInEx/config/augusdogus.mods.shieldmebruhreforged.cfg`. It starts with fresh settings and shield selection. It does not read or migrate the original mod's configuration or saved selection.

To install with r2modman, disable the original ShieldMeBruh mod, install the dependencies, and import the ZIP through **Import local mod**. Use author `AugusDogus`, name `ShieldMeBruhReforged`, and version `1.0.0`. For manual installation, remove the original mod's DLL from the profile and copy `plugins/ShieldMeBruhReforged.dll` from the ZIP into `BepInEx/plugins`. Run only one version of the mod at a time.

### Building and checking compatibility

Requires the .NET 8 SDK or newer, the current Valheim client, and a BepInEx installation. Game assemblies are publicized automatically during the build and are never packaged with the mod.

```sh
dotnet build ShieldMeBruhReforged.sln -c Release \
  -p:GameDir="/path/to/Valheim" \
  -p:BepInExDir="/path/to/profile/BepInEx"

dotnet run --project tests/CompatibilityCheck \
  -p:BepInExDir="/path/to/profile/BepInEx" -- \
  "/path/to/Valheim/valheim_Data/Managed" \
  "/path/to/profile/BepInEx" \
  "ShieldMeBruhReforged/bin/Release/netstandard2.1/ShieldMeBruhReforged.dll"

dotnet build ShieldMeBruhReforged/ShieldMeBruhReforged.csproj -c Release -t:Package \
  -p:GameDir="/path/to/Valheim" \
  -p:BepInExDir="/path/to/profile/BepInEx"
```

The package is written to `artifacts/ShieldMeBruhReforged-1.0.0-valheim-1.0.12.zip`. If your installation uses a different data directory, pass `-p:ManagedDir="/path/to/Managed"` to the build.

The compatibility check validates Harmony targets, injected parameters, compiled game references, and the embedded marker image. It does not run the game. Before considering the update game-tested, verify:

* Middle-click selection and deselection, then one-handed weapon equip and unequip.
* Moving the selected shield within the inventory, into a chest, and onto the ground.
* Saved selection after logout, death, and tombstone recovery.
* Disabling and re-enabling auto-shield, including starting with it disabled.

### Changelog
[Patch notes](CHANGELOG.md)

### Credits

Original code and artwork by [Vapok](https://github.com/Vapok), from [ShieldMeBruh](https://github.com/Vapok/ShieldMeBruh). The original [MIT license and copyright notice](LICENSE.md) are retained. Reforged is independently maintained; please do not direct fork-specific support requests to the original author.
