# Shield Me Bruh Reforged Patchnotes

## 1.0.0 - Independent fork for Valheim 1.0.12
* Rename the mod, assembly, and package to ShieldMeBruhReforged.
* Use a new plugin ID, configuration file, and character save-data key, without migrating original settings or shield selections.
* Target .NET Standard 2.1 and the current Valheim game assemblies.
* Use Valheim's new InventoryElement component for shield selection and markers.
* Replace the bundled Vapok library and Jotunn initialization with BepInEx configuration and logging.
* Keep inventory input registered when auto-shield starts disabled, so enabling it during play works.
* Add a portable build, automatic assembly publicizing, a ZIP packaging target, and static compatibility checks.

## Upstream 1.1.2 - Updating Dependencies
* Updated to latest version of dependencies

<details>
<summary><b>Changelog History</b> (<i>click to expand</i>)</summary>

## 1.1.1 - Fixing Dedicated Server Config Syncing
* A regression issue was introduced when switching to Jotunn preventing servers from dictating configs to clients.
  * This has been resolved.
* Appropriately added the BepInDependency Flags for graceful mod exit if missing dependencies.

# 1.1.0 - Removed ServerSync - Updated to Jotunn
* Updates to Valheim 0.221.4

## 1.0.9 - Valheim 0.217.28
* Updates to Valheim 0.217.28

## 1.0.8 - Valheim 0.217.24
* Updates to Valheim 0.217.24

## 1.0.7 - Valheim 0.217.14
* Updates to Valheim 0.217.14

## 1.0.6 - Valheim 0.216.9
* Updates to Valheim 0.216.9

## 1.0.5 - Valheim 0.214.2 and BepInEx 5.4.21 Updates
* Updates to Valheim 0.214.2
* Updates to BepInEx 5.4.21
* Various Clean Up

## 1.0.4 - Tombstone Retrieval Error Message
* Fixed: During Tombstone retrieval, an error would occur.

## 1.0.3 - Defensive Equip Check
* When equipping shields, make sure that the item isn't null to prevent downstream issues.
* Reinforced possible null exceptions with null checks
* Ensure shield is removed when item is moved with Fast Item Transfer

## 1.0.2 - Bug Fix
* Shield Icon resets on Death event.

## 1.0.1 - Initial Release - Hotfix
* Changed the way I'm detecting player being loaded.

## 1.0.0 - Initial Release

* Provides an option to use the middle-mouse button on a Shield while browsing the inventory to select a shield that will be automatically equipped when a one-handed weapon is equipped.
* Provides a secondary, configurable option to also automatically unequip the shield when the one-handed weapon is unequipped.
* Client side only mod.
* Lightweight, minimal overhead QoL Module

</details>
