// In-memory game and Unity collaborators. Tests execute the production feature and patches.
using ShieldMeBruhReforged.Features;
using UnityEngine;
using UnityEngine.UI;

public record struct Vector2i(int x, int y);
public class UIInputHandler
{
    public GameObject gameObject;
    public Action<UIInputHandler> m_onMiddleDown;
}
public class InventoryElement
{
    public Vector2i Position;
    public GameObject gameObject = new();
    public Image m_icon = new(), m_noteleport = new();
    public InventoryElement() => m_icon.transform.parent = gameObject.transform;
    public bool Marked => gameObject.transform.Children.Any(child => child.name == "shield" && child.GetComponent<Image>().enabled);
}
public class ItemDrop
{
    public class ItemData
    {
        public enum ItemType { Shield, OneHandedWeapon, TwoHandedWeapon }
        public class SharedData { public ItemType m_itemType; }
        public SharedData m_shared = new();
        public Vector2i m_gridPos;
        public bool m_equipped;
        public Dictionary<string, string> m_customData = [];
    }
}
public class Inventory
{
    public int m_width = 8;
    public readonly List<ItemDrop.ItemData> Items = [];
    public int GetWidth() => m_width;
    public int GetHeight() => 4;
    public List<ItemDrop.ItemData> GetAllItems() => Items;
    public ItemDrop.ItemData GetItemAt(int x, int y) => Items.Find(item => item.m_gridPos == new Vector2i(x, y));
}
public class Humanoid
{
    public ItemDrop.ItemData LastEquipped, LastUnequipped;
    public void EquipItem(ItemDrop.ItemData item) => LastEquipped = item;
    public void UnequipItem(ItemDrop.ItemData item) => LastUnequipped = item;
}
public class Player : Humanoid
{
    public static Player m_localPlayer;
    public Dictionary<string, string> m_customData = [];
    public Inventory Inventory = new();
    public Inventory GetInventory() => Inventory;
    public void SetLocalPlayer() => m_localPlayer = this;
}
public class InventoryGrid
{
    public Inventory m_inventory;
    public int m_width = 8, m_height = 4;
    public List<InventoryElement> m_elements = [];
    public InventoryElement GetElement(int x, int y, int width) => m_elements.Find(element => element.Position == new Vector2i(x, y));
    public Vector2i GetButtonPos(GameObject button) => m_elements.Find(element => element.gameObject == button).Position;
    public void UpdateGui() { }
}
namespace ShieldMeBruhReforged
{
    public static class ShieldMeBruhReforged
    {
        public const string PluginId = "test.shield";
        public static AutoShield AutoShield = new(new BepInEx.Configuration.ConfigFile());
        public static LogDouble Log = new();
    }
    public class LogDouble
    {
        public void LogDebug(string message) { }
        public void LogError(string message) => throw new InvalidOperationException(message);
    }
}
namespace BepInEx.Configuration
{
    public class ConfigFile
    {
        public ConfigEntry<T> Bind<T>(string section, string name, T value, string description) => new(value);
    }
    public class ConfigEntry<T>(T initial)
    {
        private T value = initial;
        public event EventHandler SettingChanged;
        public T Value
        {
            get => value;
            set { this.value = value; SettingChanged?.Invoke(this, EventArgs.Empty); }
        }
    }
}
namespace HarmonyLib
{
    public class HarmonyPatch : Attribute { public HarmonyPatch(Type type, string method, params Type[] arguments) { } }
    public class HarmonyPriority : Attribute { public HarmonyPriority(int value) { } }
    public static class Priority { public const int First = 800; }
}
namespace UnityEngine
{
    public class Object
    {
        public static Image Instantiate(Image source, Transform parent)
        {
            var image = new Image();
            image.transform.parent = parent;
            parent.Children.Add(image.transform);
            return image;
        }
    }
    public class Transform
    {
        public string name;
        public Transform parent;
        public object Component;
        public List<Transform> Children = [];
        public int childCount => Children.Count;
        public Transform GetChild(int index) => Children[index];
        public T GetComponent<T>() where T : class => Component as T;
    }
    public class GameObject
    {
        public string name;
        public Transform transform = new();
        private readonly UIInputHandler input;
        public GameObject() => input = new UIInputHandler { gameObject = this };
        public T GetComponentInChildren<T>() where T : class => input as T;
    }
    public record struct Vector2(float x, float y);
    public record struct Rect(float x, float y, float width, float height);
    public enum SpriteMeshType { Tight }
    public class Texture2D { public Texture2D(int width, int height) { } }
    public static class ImageConversion { public static bool LoadImage(Texture2D texture, byte[] bytes) => true; }
    public class Sprite
    {
        public static Sprite Create(Texture2D texture, Rect size, Vector2 pivot, int units, int extrude, SpriteMeshType type) => new();
    }
}
namespace UnityEngine.UI
{
    public class Image
    {
        public string name;
        public bool enabled;
        public int color, type;
        public Sprite sprite;
        public Transform transform;
        public Image() => transform = new Transform { Component = this };
    }
}
