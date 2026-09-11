using System.Globalization;
using ShieldMeBruhReforged.Features;

const string key = "test.shield";
var data = new Dictionary<string, string> { ["another.mod"] = "untouched" };
int checks = 0;

Check(ShieldSelection.Read(data, key, 8, 4) == null, "Missing selection must stay unselected.");
foreach (var slot in new[] { (0, 0), (7, 3), (2, 1) })
{
    ShieldSelection.Save(data, key, slot);
    Check(ShieldSelection.Read(data, key, 8, 4) == slot, "Selected slot must survive save and reload.");
}

ShieldSelection.Save(data, key, null);
Check(!data.ContainsKey(key), "Deselecting must remove the saved key.");
Check(ShieldSelection.Read(data, key, 8, 4) == null, "Deselecting must survive reload.");
Check(data["another.mod"] == "untouched", "Other mods' data must be preserved.");

foreach (string invalid in new[] { "", "1", "1,", ",2", "1,2,3", "a,2", "-1,0", "0,-1", "8,0", "0,4", "2147483648,0", "1.0,2", " 1,2", "+1,2", "SavedElement:\n  x: 1\n  y: 2" })
{
    data[key] = invalid;
    Check(ShieldSelection.Read(data, key, 8, 4) == null, $"Invalid selection must be ignored: {invalid}");
}

CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
ShieldSelection.Save(data, key, (7, 3));
Check(data[key] == "7,3", "Saved coordinates must be culture-independent.");
Check(ShieldSelection.Read(data, key, 8, 4) == (7, 3), "Loading must be culture-independent.");
Check(ShieldSelection.Read(data, key, 7, 3) == null, "A selection outside a resized inventory must be ignored.");

Console.WriteLine($"Passed {checks} selection checks.");

void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    checks++;
}
