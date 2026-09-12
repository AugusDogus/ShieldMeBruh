<p align="center">
  <img src="package/banner.png" alt="Shield Me Bruh Reforged: auto-equip shields for Valheim" width="900">
</p>

Automatically equip your preferred shield when you draw a one-handed weapon.

## Features

- Choose a shield with a middle-click in your inventory.
- Automatically equip it alongside a one-handed weapon.
- Optionally unequip it when you put the weapon away.
- Remember the same shield across sessions, storage, and death recovery.

## Installation

Requires **Valheim 1.0.12** and **BepInExPack_Valheim 5.4.2350**.

1. Install the dependencies in your r2modman profile.
2. Download the package from [GitHub Actions](https://github.com/AugusDogus/ShieldMeBruhReforged/actions),
   or build the project using the instructions below.
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

Your character remembers the selected shield's ID. Storing it or dying keeps the
preference; it resumes when that shield returns, even in another inventory slot.
Only deselecting or choosing another shield changes the preference. These IDs are
plain-text custom data that vanilla does not use.

## Build

Requires .NET SDK 8 and the game dependencies above.

GitHub Actions builds and checks the plugin automatically. Pushing a version tag
creates a GitHub release with the built ZIP and publishes it to Thunderstore.
Use [bumpp](docs/DEVELOPMENT.md#release) to update versions and create the tag.

```sh
dotnet build src/ShieldMeBruhReforged/ShieldMeBruhReforged.csproj -c Release \
  -p:GameDir="/path/to/Valheim" \
  -p:BepInExDir="/path/to/profile/BepInEx"
```

Output: `src/ShieldMeBruhReforged/bin/Release/netstandard2.1/ShieldMeBruhReforged.dll`.

[Development and releases](docs/DEVELOPMENT.md) · [Repository layout](docs/REPOSITORY.md) · [Changelog](CHANGELOG.md)

---

Based on [Shield Me Bruh! by Vapok](https://github.com/Vapok/ShieldMeBruh).
Original code and artwork retained under the [MIT license](LICENSE.md).
