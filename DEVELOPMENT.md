# Development

See [Build](README.md#build) for the basic build command. Game assemblies are
publicized automatically and are never packaged with the mod.

If your installation uses a different data directory, pass
`-p:ManagedDir="/path/to/Managed"` to the build.

## Compatibility checks

Run the selection persistence checks without game dependencies:

```sh
dotnet run --project tests/SelectionCheck
dotnet run --project tests/LifecycleCheck
```

These cover saved identities, deselection, invalid metadata, storage and return,
logout and respawn, tombstone recovery into a different slot, and marker refresh
without a grid resize. Lifecycle checks execute the production feature and patches
with in-memory game and Unity collaborators.

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
- Moving the selected shield within the inventory, storing or dropping it, and retrieving it in a different slot.
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

## GitHub Actions

The **Build and publish** workflow compiles the plugin on GitHub for pushes to
`main`, pull requests, manual runs, and published releases. It downloads current
public Valheim dedicated-server assemblies through anonymous SteamCMD and the
BepInEx version listed in `manifest.json`. No local game installation or Steam
credentials are needed. This follows [Jötunn's CI approach](https://github.com/Valheim-Modding/Jotunn/blob/master/.github/workflows/pull-request.yml).

Each build runs the selection and compatibility checks, validates the package,
and uploads a `thunderstore-package` Actions artifact. Game assemblies are not
included. These checks do not replace in-game testing.

For publishing, configure these repository Actions settings:

- Variable `THUNDERSTORE_NAMESPACE`: `AugusDogus`.
- Secret `TCLI_AUTH_TOKEN`: a service-account token for that Thunderstore team.
  Create it under Thunderstore **Settings > Teams > AugusDogus > Service Accounts**.

Keep version `1.0.0` until the first public release; commits identify development
builds. For later releases, update the version in `manifest.json`, the project file,
`BepInPlugin`, and both assembly version attributes, then update the changelog.
Commit and push, then publish a stable GitHub release tagged `v1.0.0` or `1.0.0`
(using the matching version).
The tag must match the source versions. The workflow builds the tagged source,
attaches its ZIP to the release, and uploads that same ZIP using Thunderstore's
official CLI. You do not need to build or attach anything manually.

Pull requests, ordinary pushes, manual runs, and prereleases only build and check;
they do not publish to Thunderstore. If publishing fails, the built artifact remains
available. After correcting missing credentials or settings, rerun the failed job.
For a version already published to Thunderstore, make a new version instead.

## Plugin identity

The plugin ID and character save-data key are `augusdogus.mods.shieldmebruhreforged`.
Configuration is stored at `BepInEx/config/augusdogus.mods.shieldmebruhreforged.cfg`.
Reforged does not read or migrate the original mod's configuration or saved selection.

The character preference and selected item's ID use the plugin key in their
respective `m_customData` dictionaries. Values are GUIDs in `N` format. Valheim's
normal character and item serialization persists them. Only explicit deselection
clears the character preference; leaving the inventory does not. A missing shield
is inactive until its ID is found again, regardless of its inventory coordinates.
Invalid values mean no selection. Earlier development builds' slot/YAML values
are not migrated; select the shield once when switching from those builds.
No serialization library is required.
