using System.Globalization;
using ShieldMeBruhReforged.Features;

const string key = "test.shield";
var character = new Dictionary<string, string> { ["another.mod"] = "untouched" };
var shield = new Dictionary<string, string> { ["another.item.mod"] = "untouched" };
var otherShield = new Dictionary<string, string>();
int checks = 0;

Check(!ShieldSelection.IsSelected(character, shield, key), "Missing IDs must not match.");
ShieldSelection.Select(character, shield, key);
Check(ShieldSelection.IsSelected(character, shield, key), "Selecting must match the character to that shield.");
Check(!ShieldSelection.IsSelected(character, otherShield, key), "Other shields must remain unselected.");
var id = shield[key];
ShieldSelection.Select(character, shield, key);
Check(shield[key] == id, "Reselecting must preserve the item's identity.");
Check(ShieldSelection.IsSelected(new Dictionary<string, string>(character), new Dictionary<string, string>(shield), key), "Serialized and reloaded character/item metadata must retain the preference.");
ShieldSelection.Select(character, otherShield, key);
Check(ShieldSelection.IsSelected(character, otherShield, key) && !ShieldSelection.IsSelected(character, shield, key), "Selecting another shield must replace the preference.");
ShieldSelection.Clear(character, key);
Check(!character.ContainsKey(key) && !ShieldSelection.IsSelected(character, otherShield, key), "Deselecting must clear the character preference.");
Check(shield[key] == id, "Deselecting must not erase the item's identity.");
Check(character["another.mod"] == "untouched" && shield["another.item.mod"] == "untouched", "Unrelated metadata must be preserved.");

foreach (string invalid in new[] { "", "1", "2,1", "not-an-id", new string('0', 32), new string('g', 32), "SavedElement:\n  x: 1\n  y: 2" })
{
    character[key] = invalid;
    Check(ShieldSelection.Read(character, key) == null, "Malformed and old-format preferences must be ignored.");
}
CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("ar-SA");
ShieldSelection.Select(character, shield, key);
Check(character[key] == id, "IDs must be culture-independent.");
Console.WriteLine($"Passed {checks} selection checks.");

void Check(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
    checks++;
}
