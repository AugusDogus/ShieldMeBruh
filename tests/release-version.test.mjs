import { afterEach, expect, test } from 'bun:test';
import { mkdtempSync, mkdirSync, readFileSync, rmSync, writeFileSync } from 'node:fs';
import { tmpdir } from 'node:os';
import { dirname, join } from 'node:path';
import { readVersion, updateVersions } from '../scripts/release-version.mjs';

const roots = [];
afterEach(() => {
  for (const root of roots.splice(0)) rmSync(root, { recursive: true, force: true });
});

function fixture() {
  const root = mkdtempSync(join(tmpdir(), 'shield-version-'));
  roots.push(root);
  const files = {
    'manifest.json': '{"version_number":"1.0.1","description":"Valheim 1.0.12","dependencies":["Other-1.0.1"]}\n',
    'ShieldMeBruhReforged/ShieldMeBruhReforged.csproj': '<Project><Version>1.0.1</Version><Reference Version="1.0.1" /></Project>\n',
    'ShieldMeBruhReforged/ShieldMeBruhReforged.cs': '\uFEFF[BepInPlugin(PluginId, "Shield Me Bruh Reforged", "1.0.1")]\r\n',
    'ShieldMeBruhReforged/Properties/AssemblyInfo.cs': '// [assembly: AssemblyVersion("1.0.*")]\n[assembly: AssemblyVersion("1.0.1.0")]\n[assembly: AssemblyFileVersion("1.0.1.0")]\n// Valheim 1.0.12\n',
  };
  for (const [name, text] of Object.entries(files)) {
    mkdirSync(dirname(join(root, name)), { recursive: true });
    writeFileSync(join(root, name), text);
  }
  return { root, files };
}

test('updates all mod versions while preserving dependencies, game versions and formatting', () => {
  const { root } = fixture();
  expect(updateVersions('2.3.4', root)).toHaveLength(4);
  expect(readVersion(root)).toBe('2.3.4');
  expect(readFileSync(join(root, 'manifest.json'), 'utf8')).toBe('{"version_number":"2.3.4","description":"Valheim 1.0.12","dependencies":["Other-1.0.1"]}\n');
  expect(readFileSync(join(root, 'ShieldMeBruhReforged/ShieldMeBruhReforged.csproj'), 'utf8')).toBe('<Project><Version>2.3.4</Version><Reference Version="1.0.1" /></Project>\n');
  expect(readFileSync(join(root, 'ShieldMeBruhReforged/ShieldMeBruhReforged.cs'), 'utf8')).toBe('\uFEFF[BepInPlugin(PluginId, "Shield Me Bruh Reforged", "2.3.4")]\r\n');
  expect(readFileSync(join(root, 'ShieldMeBruhReforged/Properties/AssemblyInfo.cs'), 'utf8')).toBe('// [assembly: AssemblyVersion("1.0.*")]\n[assembly: AssemblyVersion("2.3.4.0")]\n[assembly: AssemblyFileVersion("2.3.4.0")]\n// Valheim 1.0.12\n');
});

test('unchanged version supports tagging the first release without a version bump', () => {
  const { root } = fixture();
  expect(updateVersions('1.0.1', root)).toEqual([]);
});

test.each(['1.0.1-rc.1', '01.2.3', '1.2', '65535.0.0', 'invalid'])('rejects unsupported version %s without edits', version => {
  const { root, files } = fixture();
  expect(() => updateVersions(version, root)).toThrow();
  for (const [name, text] of Object.entries(files)) expect(readFileSync(join(root, name), 'utf8')).toBe(text);
});

test.each(['inconsistent', 'duplicate'])('rejects %s metadata before any writes', problem => {
  const { root, files } = fixture();
  const name = 'ShieldMeBruhReforged/Properties/AssemblyInfo.cs';
  files[name] = problem === 'inconsistent'
    ? files[name].replace('AssemblyFileVersion("1.0.1.0")', 'AssemblyFileVersion("1.0.0.0")')
    : files[name] + '[assembly: AssemblyFileVersion("1.0.1.0")]\n';
  writeFileSync(join(root, name), files[name]);
  expect(() => updateVersions('1.0.2', root)).toThrow();
  for (const [name, text] of Object.entries(files)) expect(readFileSync(join(root, name), 'utf8')).toBe(text);
});
