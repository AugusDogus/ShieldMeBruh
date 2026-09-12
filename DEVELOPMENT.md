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
`main`, pull requests, manual runs, and version tags. It downloads current
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
builds. Commit your changes and changelog, then use [bumpp](https://github.com/antfu-collective/bumpp)
to update the version fields, create a release commit, and tag it:

```sh
npx bumpp@12.3.0 --release 1.0.0
git push --atomic reforged HEAD:main v1.0.0
```

For later releases, omit `--release` to choose interactively, or use
`--release patch`, `--release minor`, or an explicit version. Push the resulting
tag in place of `v1.0.0`. Requires Node.js 22.18+ or 24.11+; no `package.json` is
needed. The config updates `manifest.json`, the project version, `BepInPlugin`,
and both assembly version attributes without changing game or dependency versions.
Run its regression checks with `bun test tests/release-version.test.mjs`.

bumpp requires a clean working tree and leaves pushing to the explicit command
above. In this checkout, `reforged` points to our private repo and `origin` points
to the original mod. For releases after `1.0.0`, use a fresh clone of the Reforged
repo to avoid collisions with inherited upstream tags; that clone uses `origin`
instead of `reforged`. Existing tags are never overwritten.

Tags must use `vMAJOR.MINOR.PATCH` and match the source versions. The workflow
builds and checks the tagged source, creates a GitHub release with generated notes
and the ZIP attached, then uploads that same ZIP using Thunderstore's official CLI.
You do not need to build, create the release, or attach anything manually.

Pull requests, branch pushes, and manual runs only build and check. Prerelease
tags such as `v1.0.0-rc.1` do not trigger the workflow. Manually publishing a GitHub
release does not trigger it either. If publishing fails, the built artifact remains
available. After correcting missing credentials or settings, rerun the failed job;
it reuses an existing GitHub release and replaces its ZIP. For a version already
published to Thunderstore, make a new version instead.

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
