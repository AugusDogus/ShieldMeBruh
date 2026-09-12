#!/usr/bin/env bash
set -euo pipefail
cd "$(dirname "$0")/.."
: "${MANAGED_DIR:?Set MANAGED_DIR to the Valheim Managed directory.}"
: "${BEPINEX_DIR:?Set BEPINEX_DIR to the profile BepInEx directory.}"

dotnet run --project tests/SelectionCheck -c Release
dotnet run --project tests/LifecycleCheck -c Release
dotnet run --project tests/CompatibilityCheck -c Release \
  -p:BepInExDir="$BEPINEX_DIR" -- \
  "$MANAGED_DIR" "$BEPINEX_DIR" \
  src/ShieldMeBruhReforged/bin/Release/netstandard2.1/ShieldMeBruhReforged.dll
