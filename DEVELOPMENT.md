# Development

See [Build](README.md#build) for the basic build command. Game assemblies are
publicized automatically and are never packaged with the mod.

If your installation uses a different data directory, pass
`-p:ManagedDir="/path/to/Managed"` to the build.

## Compatibility checks

Run the selection persistence checks without game dependencies:

```sh
dotnet run --project tests/SelectionCheck
```

These cover round trips, deselection, malformed and out-of-bounds coordinates,
culture-independent formatting, and preservation of unrelated metadata.

Check the compiled plugin against your installed game:

```sh
dotnet run --project tests/CompatibilityCheck \
  -p:BepInExDir="/path/to/profile/BepInEx" -- \
  "/path/to/Valheim/valheim_Data/Managed" \
  "/path/to/profile/BepInEx" \
  "ShieldMeBruhReforged/bin/Release/netstandard2.1/ShieldMeBruhReforged.dll"
```

This validates Harmony targets, injected parameters, compiled game references,
and the embedded marker image. It does not run the game.

Before considering the update game-tested, verify:

- Middle-click selection and deselection, then one-handed weapon equip and unequip.
- Moving the selected shield within the inventory, into a chest, and onto the ground.
- Saved selection after logout, death, and tombstone recovery.
- Disabling and re-enabling auto-shield, including starting with it disabled.

## Packaging

```sh
dotnet build ShieldMeBruhReforged/ShieldMeBruhReforged.csproj -c Release -t:Package \
  -p:GameDir="/path/to/Valheim" \
  -p:BepInExDir="/path/to/profile/BepInEx"
```

Output: `artifacts/ShieldMeBruhReforged-1.0.0-valheim-1.0.12.zip`.

Import the ZIP through r2modman's **Import local mod**. Use author
`AugusDogus`, name `ShieldMeBruhReforged`, and version `1.0.0`.

## Plugin identity

The plugin ID and character save-data key are `augusdogus.mods.shieldmebruhreforged`.
Configuration is stored at `BepInEx/config/augusdogus.mods.shieldmebruhreforged.cfg`.
Reforged does not read or migrate the original mod's configuration or saved selection.

The selection value is `x,y` with invariant integer formatting in `Player.m_customData`.
Valheim's existing character save/load code persists it. Missing, malformed, and
out-of-bounds values mean no selection; deselecting removes the key. Earlier YAML
values are not migrated. No serialization library is required.
