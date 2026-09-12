using System;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShieldMeBruhReforged.Features;

public class AutoShield : IDisposable
{
    private InventoryGrid _activeInstance;

    private Sprite _shield;
    private InventoryElement _currentElement;

    public ConfigEntry<bool> EnableAutoShield;
    public ConfigEntry<bool> EnableAutoUnequip;
    
    public AutoShield(ConfigFile config)
    {
        EnableAutoShield = config.Bind("Local Config", "Enable Auto Shield", true,
            "When enabled, selected shield will automatically equip when a one handed weapon is equipped.");
        EnableAutoUnequip = config.Bind("Local Config", "Enable Auto Unequip", true,
            "When enabled, when one handed weapon is unequipped, the marked equipped shield, will also unequip.");
        EnableAutoShield.SettingChanged += OnEnabledChanged;
    }

    private void OnEnabledChanged(object sender, EventArgs args)
    {
        if (_activeInstance != null) RefreshSelection(_activeInstance);
    }

    public void Dispose() => EnableAutoShield.SettingChanged -= OnEnabledChanged;

    public void LoadAssets()
    {
        var path = "ShieldMeBruhReforged.Resources";
        _shield = LoadSprite($"{path}.shield.png", new Rect(0, 0, 1024, 1024));
    }

    public void SetActiveInstance(InventoryGrid instance)
    {
        _activeInstance = instance;
    }

    public static Texture2D LoadImage(byte[] bytes)
    {
        var texture = new Texture2D(2, 2);
    
        var isSuccess = ImageConversion.LoadImage(texture, bytes);
    
        if (!isSuccess)
            throw new Exception("Failed to load image data into texture from byte array");
    
        return texture;
    }

    public Sprite LoadSprite(string path, Rect size, Vector2? pivot = null, int units = 100)
    {
        if (pivot == null) pivot = new Vector2(0.5f, 0.5f);

        var assembly = Assembly.GetExecutingAssembly();
        var imageStream = assembly.GetManifestResourceStream(path);

        var imageData = ReadToEnd(imageStream);
        var texture = LoadImage(imageData);

        if (texture == null) ShieldMeBruhReforged.Log.LogError("Missing Embedded Resource: " + path);

        return Sprite.Create(texture, size, pivot.Value, units, 0, SpriteMeshType.Tight);
    }

    private byte[] ReadToEnd(Stream stream)
    {
        var originalPosition = stream.Position;
        stream.Position = 0;

        try
        {
            var readBuffer = new byte[4096];

            var totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readBuffer, totalBytesRead, readBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readBuffer.Length)
                {
                    var nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        var temp = new byte[readBuffer.Length * 2];
                        Buffer.BlockCopy(readBuffer, 0, temp, 0, readBuffer.Length);
                        Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            var buffer = readBuffer;
            if (readBuffer.Length != totalBytesRead)
            {
                buffer = new byte[totalBytesRead];
                Buffer.BlockCopy(readBuffer, 0, buffer, 0, totalBytesRead);
            }

            return buffer;
        }
        finally
        {
            stream.Position = originalPosition;
        }
    }

    private Image CreateShieldedImage(Image baseImg, Image noTeleport)
    {
        // set m_queued parent as parent first, so the position is correct
        var obj = Object.Instantiate(baseImg, baseImg.transform.parent);
        // change the parent to the m_queued image so we can access the new image without a loop
        var transform = obj.transform;
        //transform.SetParent(baseImg.transform);
        transform.name = "shield";
        //transform.SetAsLastSibling();

        // set the new shield image
        obj.sprite = _shield;
        obj.name = "shield";
        obj.color = noTeleport.color;
        obj.type = noTeleport.type;

        return obj;
    }

    public ItemDrop.ItemData GetSelectedShield(Player player)
    {
        foreach (var item in player.GetInventory().GetAllItems())
        {
            if (item.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Shield &&
                ShieldSelection.IsSelected(player.m_customData, item.m_customData, ShieldMeBruhReforged.PluginId))
                return item;
        }
        return null;
    }

    public void OnMiddleClick(UIInputHandler middleClick)
    {
        if (!EnableAutoShield.Value || Player.m_localPlayer is not { } player ||
            _activeInstance == null || middleClick == null)
            return;

        var inventory = player.GetInventory();
        if (_activeInstance.m_inventory != inventory) return;

        var position = _activeInstance.GetButtonPos(middleClick.gameObject);
        var item = inventory.GetItemAt(position.x, position.y);
        if (item == null || item.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield) return;

        var previous = GetSelectedShield(player);
        if (previous == item)
            ShieldSelection.Clear(player.m_customData, ShieldMeBruhReforged.PluginId);
        else
            ShieldSelection.Select(player.m_customData, item.m_customData, ShieldMeBruhReforged.PluginId);

        RefreshSelection(_activeInstance);
        if (previous != null && previous != item && previous.m_equipped)
            player.EquipItem(item);
    }

    public void RefreshSelection(InventoryGrid grid)
    {
        if (Player.m_localPlayer is not { } player || grid.m_inventory != player.GetInventory()) return;
        _activeInstance = grid;

        // The saved identity survives storage and death; only the marker depends on its current slot.
        var item = GetSelectedShield(player);
        InventoryElement element = null;
        if (item != null && item.m_gridPos.x >= 0 && item.m_gridPos.x < grid.m_width &&
            item.m_gridPos.y >= 0 && item.m_gridPos.y < grid.m_height)
            element = grid.GetElement(item.m_gridPos.x, item.m_gridPos.y, grid.m_width);

        if (_currentElement != null && _currentElement != element)
            GetShield(_currentElement).enabled = false;
        _currentElement = element;
        if (_currentElement != null)
            GetShield(_currentElement).enabled = EnableAutoShield.Value;
    }

    public void ResetPlayerContext()
    {
        if (_currentElement != null) GetShield(_currentElement).enabled = false;
        _currentElement = null;
        _activeInstance = null;
    }

    private Image GetShield(InventoryElement element)
    {
        Image img = null;

        if (element.gameObject == null)
        {
            ShieldMeBruhReforged.Log.LogError("Element.gameObject is null");
            return null;
        }

        if (element.gameObject.transform.childCount > 0)
        {
            for (var i = 0; i < element.gameObject.transform.childCount; i++)
            {
                var childTransform = element.gameObject.transform.GetChild(i);
                var childImage = childTransform.GetComponent<Image>();
                
                if (childImage != null)
                {
                    if (childImage.transform.name == "shield")
                        img = childImage;
                }
            }
        }

        if (img == null)
        {
            ShieldMeBruhReforged.Log.LogDebug($"Image Null: {element.gameObject.transform.name}");
            img = CreateShieldedImage(element.m_icon, element.m_noteleport);
        }

        return img;
    }

}
