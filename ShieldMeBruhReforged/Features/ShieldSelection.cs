using System;
using System.Collections.Generic;

namespace ShieldMeBruhReforged.Features;

internal static class ShieldSelection
{
    // Both character and item custom data are persisted by Valheim as ordinary strings.
    public static Guid? Read(IDictionary<string, string> data, string key)
    {
        return data.TryGetValue(key, out var value) && Guid.TryParseExact(value, "N", out var id) && id != Guid.Empty
            ? id
            : null;
    }

    public static bool IsSelected(IDictionary<string, string> character, IDictionary<string, string> item, string key)
    {
        return Read(character, key) is { } selected && Read(item, key) == selected;
    }

    public static void Select(IDictionary<string, string> character, IDictionary<string, string> item, string key)
    {
        var id = Read(item, key) ?? Guid.NewGuid();
        item[key] = id.ToString("N");
        character[key] = item[key];
    }

    public static void Clear(IDictionary<string, string> character, string key) => character.Remove(key);
}
