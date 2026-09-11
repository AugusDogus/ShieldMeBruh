using System;
using System.Collections.Generic;
using System.Globalization;

namespace ShieldMeBruhReforged.Features;

internal static class ShieldSelection
{
    // Valheim persists this dictionary as strings in the character save.
    public static (int X, int Y)? Read(IDictionary<string, string> data, string key, int width, int height)
    {
        if (!data.TryGetValue(key, out var value) || string.IsNullOrEmpty(value))
            return null;

        var coordinates = value.Split(',');
        if (coordinates.Length != 2 ||
            !int.TryParse(coordinates[0], NumberStyles.None, CultureInfo.InvariantCulture, out var x) ||
            !int.TryParse(coordinates[1], NumberStyles.None, CultureInfo.InvariantCulture, out var y) ||
            x < 0 || x >= width || y < 0 || y >= height)
            return null;

        return (x, y);
    }

    public static void Save(IDictionary<string, string> data, string key, (int X, int Y)? selection)
    {
        if (selection is { } slot)
            data[key] = FormattableString.Invariant($"{slot.X},{slot.Y}");
        else
            data.Remove(key);
    }
}
