<p align="center">
  <img src="banner.png" alt="Shield Me Bruh Reforged: auto-equip shields for Valheim" width="900">
</p>

Automatically equip your preferred shield when you draw a one-handed weapon.

## Features

- Choose a shield with a middle-click in your inventory.
- Automatically equip it alongside a one-handed weapon.
- Optionally unequip it when you put the weapon away.
- Remember your selection between sessions.

## Installation

Requires **Valheim 1.0.12** and **BepInExPack_Valheim 5.4.2350**.

1. Install the dependencies in your r2modman profile.
2. Build the project using the instructions below.
3. Close Valheim and copy `ShieldMeBruhReforged.dll` into your profile's
   `BepInEx/plugins/ShieldMeBruhReforged/` directory.
4. Launch with **Start modded**.

Disable or remove the original ShieldMeBruh first. Reforged starts with fresh
settings and shield selection.

Compilation and compatibility checks pass; in-game testing is still pending.

## Controls

**Middle-click** a shield in your inventory to select it. Click it again to
deselect. A shield marker shows your selection.

Auto-equip and auto-unequip can be toggled in the mod's BepInEx configuration.

Your selection is stored as plain-text coordinates in the character's custom
data. Deselecting removes the entry. Vanilla does not use this mod-specific entry.

## Build

Requires .NET SDK 8 or later and the game dependencies above.

```sh
dotnet build ShieldMeBruhReforged.sln -c Release \
  -p:GameDir="/path/to/Valheim" \
  -p:BepInExDir="/path/to/profile/BepInEx"
```

Output: `ShieldMeBruhReforged/bin/Release/netstandard2.1/ShieldMeBruhReforged.dll`.

[Development instructions](DEVELOPMENT.md) · [Changelog](CHANGELOG.md)

---

Based on [Shield Me Bruh! by Vapok](https://github.com/Vapok/ShieldMeBruh).
Original code and artwork retained under the [MIT license](LICENSE.md).
