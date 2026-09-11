"""Check release metadata and the exact ZIP that CI will publish."""

import json
import os
from pathlib import Path
import re
import sys
import xml.etree.ElementTree as ET
import zipfile


def validate() -> str:
    manifest = json.loads(Path("manifest.json").read_text())
    version = manifest["version_number"]
    if not re.fullmatch(r"(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)", version):
        raise ValueError("manifest.json must contain a three-part numeric version.")

    project = Path("ShieldMeBruhReforged")
    project_version = ET.parse(project / "ShieldMeBruhReforged.csproj").findtext(".//Version")
    plugin = (project / "ShieldMeBruhReforged.cs").read_text(encoding="utf-8-sig")
    assembly = (project / "Properties/AssemblyInfo.cs").read_text(encoding="utf-8-sig")
    if project_version != version or f'"Shield Me Bruh Reforged", "{version}")' not in plugin:
        raise ValueError("The project and BepInPlugin versions must match manifest.json.")
    for attribute in ("AssemblyVersion", "AssemblyFileVersion"):
        if f'{attribute}("{version}.0")' not in assembly:
            raise ValueError(f"{attribute} must match manifest.json with a .0 revision.")

    tag = os.environ.get("RELEASE_TAG", "")
    if tag and tag not in (version, f"v{version}"):
        raise ValueError(f"Release tag {tag!r} must be {version!r} or 'v{version}'.")

    archives = list(Path("artifacts").glob("ShieldMeBruhReforged-*.zip"))
    if len(archives) != 1:
        raise ValueError(f"Expected one release ZIP, found {len(archives)}. Remove stale packages and rebuild.")
    with zipfile.ZipFile(archives[0]) as package:
        if package.testzip() is not None:
            raise ValueError("The package ZIP failed its integrity check. Rebuild it.")
        if json.loads(package.read("manifest.json")) != manifest:
            raise ValueError("The packaged manifest differs from the source manifest. Rebuild it.")
        dll = "plugins/ShieldMeBruhReforged.dll"
        if [name for name in package.namelist() if name.lower().endswith(".dll")] != [dll]:
            raise ValueError("The package must contain only the plugin DLL, without game or dependency DLLs.")
        if package.read(dll) != (project / "bin/Release/netstandard2.1/ShieldMeBruhReforged.dll").read_bytes():
            raise ValueError("The packaged plugin differs from the compiled DLL. Rebuild the package.")
        for filename in ("README.md", "icon.png", "banner.png", "LICENSE.md"):
            if package.read(filename) != Path(filename).read_bytes():
                raise ValueError(f"The packaged {filename} differs from its source. Rebuild the package.")
    return version


if __name__ == "__main__":
    try:
        version = validate()
    except (ValueError, KeyError, OSError, ET.ParseError, zipfile.BadZipFile) as error:
        print(f"Package validation failed: {error}", file=sys.stderr)
        sys.exit(1)
    if output := os.environ.get("GITHUB_OUTPUT"):
        with open(output, "a") as stream:
            stream.write(f"version={version}\n")
    print(f"Validated ShieldMeBruhReforged {version}.")
