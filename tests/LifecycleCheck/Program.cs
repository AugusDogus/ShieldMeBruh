using System.Reflection;
using ShieldMeBruhReforged.Patches;
using Plugin = ShieldMeBruhReforged.ShieldMeBruhReforged;

int checks = 0;
var player = new Player();
Player.m_localPlayer = player;
var grid = CreateGrid(player.Inventory);
var shield = new ItemDrop.ItemData { m_gridPos = new Vector2i(2, 1) };
var otherShield = new ItemDrop.ItemData { m_gridPos = new Vector2i(3, 1) };
var weapon = new ItemDrop.ItemData { m_shared = new() { m_itemType = ItemDrop.ItemData.ItemType.OneHandedWeapon } };
player.Inventory.Items.AddRange([shield, otherShield]);
Refresh(resized: true);
Click(shield);
Check(Selected() == shield && Element(shield).Marked, "Middle-click must select and mark the shield.");

// Storage must preserve the preference, without selecting a replacement in its old slot.
player.Inventory.Items.Remove(shield);
otherShield.m_gridPos = shield.m_gridPos;
Refresh();
Check(Selected() == null && !Element(otherShield).Marked, "An absent shield must not select another shield in its old slot.");
Check(player.m_customData.ContainsKey(Plugin.PluginId), "Storing the shield must preserve the preference.");
EquipWeapon(player);
Check(player.LastEquipped == null, "An absent shield must not auto-equip a substitute.");
shield.m_gridPos = new Vector2i(6, 2);
player.Inventory.Items.Add(shield);
EquipWeapon(player);
Check(player.LastEquipped == shield, "Returning the shield to another slot must restore auto-equip before opening inventory.");
Refresh();
Check(Element(shield).Marked && !Element(otherShield).Marked, "The marker must follow the preferred shield's new slot.");

// Both the player and item are fresh objects after loading a character.
var savedCharacter = new Dictionary<string, string>(player.m_customData);
shield = new ItemDrop.ItemData { m_gridPos = new Vector2i(1, 3), m_customData = new(shield.m_customData) };
player = new Player { m_customData = new(savedCharacter) };
player.Inventory.Items.Add(shield);
Player.m_localPlayer = player;
Invoke(typeof(Player_Patches), "SetLocalPlayerPatch", "Postfix", player);
EquipWeapon(player);
Check(player.LastEquipped == shield, "Loading fresh player and item objects must restore auto-equip without UI state.");
grid.m_inventory = player.Inventory;
Refresh();
Check(Element(shield).Marked, "Login must restore the marker even if the grid size is unchanged.");

// Death leaves the preference on a new player while the shield is in a grave.
var graveShield = shield;
player = new Player { m_customData = new(player.m_customData) };
Player.m_localPlayer = player;
Invoke(typeof(Player_Patches), "SetLocalPlayerPatch", "Postfix", player);
grid.m_inventory = player.Inventory;
Refresh();
Check(Selected() == null && player.m_customData.ContainsKey(Plugin.PluginId), "Respawn with an empty inventory must preserve the choice.");
shield = new ItemDrop.ItemData { m_gridPos = new Vector2i(7, 0), m_customData = new(graveShield.m_customData) };
player.Inventory.Items.Add(shield);
Refresh();
EquipWeapon(player);
Check(Element(shield).Marked && player.LastEquipped == shield, "Recovering cloned grave items in a different slot must restore both behaviors.");

var remote = new Player();
Invoke(typeof(Humanoid_Patches), "HumanoidEquipItemPatch", "Postfix", remote, weapon, true, null, true);
Check(remote.LastEquipped == null, "Another player's equipment must not be changed.");
Invoke(typeof(Humanoid_Patches), "HumanoidUnequipItemPatch", "Postfix", player, weapon, shield, true);
Check(player.LastUnequipped == shield, "Putting away a weapon must unequip the preferred shield.");

otherShield.m_gridPos = new Vector2i(4, 2);
player.Inventory.Items.Add(otherShield);
player.Inventory.Items.Remove(otherShield);
Refresh();
Check(Selected() == shield, "Moving an unrelated item must preserve the selected shield.");
player.Inventory.Items.Add(otherShield);
Click(otherShield);
Check(Selected() == otherShield && !Element(shield).Marked, "Choosing another shield must replace the preference.");
Click(otherShield);
Check(Selected() == null && !player.m_customData.ContainsKey(Plugin.PluginId), "Only explicit deselection clears the preference.");
Refresh();
Check(!Element(otherShield).Marked, "Deselection must remain cleared on subsequent UI updates.");

Plugin.AutoShield.EnableAutoShield.Value = false;
grid = CreateGrid(player.Inventory);
Refresh(resized: true);
var input = Element(shield).gameObject.GetComponentInChildren<UIInputHandler>();
Check(input.m_onMiddleDown != null, "Middle-click input must be registered while the feature is disabled.");
input.m_onMiddleDown(input);
Check(Selected() == null, "Clicking while disabled must not change the preference.");
Plugin.AutoShield.EnableAutoShield.Value = true;
input.m_onMiddleDown(input);
Check(Selected() == shield, "The registered handler must select exactly once after re-enabling.");
Plugin.AutoShield.EnableAutoShield.Value = false;
Check(!Element(shield).Marked, "Disabling must hide the marker without forgetting the choice.");
player.LastEquipped = null;
EquipWeapon(player);
Check(player.LastEquipped == null, "Disabling must stop auto-equip.");
Plugin.AutoShield.EnableAutoShield.Value = true;
Check(Selected() == shield && Element(shield).Marked, "Re-enabling must restore the selected shield.");

Console.WriteLine($"Passed {checks} lifecycle checks against production feature and patches.");

ItemDrop.ItemData Selected() => Plugin.AutoShield.GetSelectedShield(player);
InventoryElement Element(ItemDrop.ItemData item) => grid.GetElement(item.m_gridPos.x, item.m_gridPos.y, grid.m_width);
void Click(ItemDrop.ItemData item) => Plugin.AutoShield.OnMiddleClick(Element(item).gameObject.GetComponentInChildren<UIInputHandler>());
void Refresh(bool resized = false) => Invoke(typeof(InventoryGrid_Patches), "InventoryGridUpdateGuiPatch", "Postfix", grid, resized, true);
void EquipWeapon(Player target) => Invoke(typeof(Humanoid_Patches), "HumanoidEquipItemPatch", "Postfix", target, weapon, true, null, true);
void Check(bool success, string message)
{
    if (!success) throw new InvalidOperationException(message);
    checks++;
}
static InventoryGrid CreateGrid(Inventory inventory)
{
    var grid = new InventoryGrid { m_inventory = inventory };
    for (int y = 0; y < 4; y++)
    for (int x = 0; x < 8; x++)
        grid.m_elements.Add(new InventoryElement { Position = new Vector2i(x, y) });
    return grid;
}
static void Invoke(Type owner, string nestedName, string methodName, params object[] arguments)
{
    const BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic;
    Type nested = owner.GetNestedType(nestedName, flags) ?? throw new InvalidOperationException(nestedName);
    MethodInfo method = nested.GetMethod(methodName, flags | BindingFlags.Static) ?? throw new InvalidOperationException(methodName);
    method.Invoke(null, arguments);
}
