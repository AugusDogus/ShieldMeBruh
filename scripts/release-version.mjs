import { readFileSync, writeFileSync } from 'node:fs';
import { resolve } from 'node:path';

const fields = [
  ['manifest.json', /("version_number"\s*:\s*")([^"]+)(")/g],
  ['ShieldMeBruhReforged/ShieldMeBruhReforged.csproj', /(<Version>)([^<]+)(<\/Version>)/g],
  ['ShieldMeBruhReforged/ShieldMeBruhReforged.cs', /(\[BepInPlugin\(PluginId, "Shield Me Bruh Reforged", ")([^"]+)("\)\])/gm],
  ['ShieldMeBruhReforged/Properties/AssemblyInfo.cs', /^(\[assembly: AssemblyVersion\(")([^"]+)("\)\])/gm],
  ['ShieldMeBruhReforged/Properties/AssemblyInfo.cs', /^(\[assembly: AssemblyFileVersion\(")([^"]+)("\)\])/gm],
];

export function readVersion(root = process.cwd()) {
  const manifest = JSON.parse(readFileSync(resolve(root, 'manifest.json'), 'utf8'));
  const version = manifest?.version_number;
  validateVersion(version);
  return version;
}

function validateVersion(version) {
  if (typeof version !== 'string'
      || !/^(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)\.(0|[1-9][0-9]*)$/.test(version)
      || version.split('.').some(part => Number(part) > 65534)) {
    throw new Error('Use MAJOR.MINOR.PATCH with each number between 0 and 65534; prerelease versions are not supported. No version files were changed.');
  }
}

export function updateVersions(version, root = process.cwd()) {
  validateVersion(version);
  const previous = readVersion(root);
  const updates = new Map();

  // Validate every field before writing, and replace only mod version metadata.
  for (const [filename, pattern] of fields) {
    const path = resolve(root, filename);
    const source = updates.get(path) ?? readFileSync(path, 'utf8');
    const matches = [...source.matchAll(pattern)];
    const assembly = filename.endsWith('AssemblyInfo.cs');
    const expected = assembly ? `${previous}.0` : previous;
    if (matches.length !== 1 || matches[0]?.[2] !== expected) {
      throw new Error(`${filename}: expected exactly one matching version field containing ${expected}. Fix the metadata before releasing. No version files were changed.`);
    }
    const next = assembly ? `${version}.0` : version;
    updates.set(path, source.replace(pattern, (_match, prefix, _old, suffix) => `${prefix}${next}${suffix}`));
  }

  const changed = [];
  for (const [path, source] of updates) {
    if (readFileSync(path, 'utf8') !== source) {
      writeFileSync(path, source);
      changed.push(path);
    }
  }
  return changed;
}
