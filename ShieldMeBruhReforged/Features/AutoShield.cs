using System;
using System.IO;
using System.Reflection;
using BepInEx.Configuration;
using ShieldMeBruhReforged.Patches;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace ShieldMeBruhReforged.Features;

public class AutoShield : IDisposable
{
    private InventoryGrid _activeInstance;

    private Sprite _shield;
    public InventoryElement CurrentElement;
    public bool FeatureInitialized = false;
    public ItemDrop.ItemData SelectedShield;

    public ConfigEntry<bool> EnableAutoShield;
    public ConfigEntry<bool> EnableAutoUnequip;
    
    public AutoShield(ConfigFile config)
    {
        EnableAutoShield = config.Bind("Local Config", "Enable Auto Shield", true,
            "When enabled, selected shield will automatically equip when a one handed weapon is equipped.");
        EnableAutoUnequip = config.Bind("Local Config", "Enable Auto Unequip", true,
            "When enabled, when one handed weapon is unequipped, the marked equipped shield, will also unequip.");
        EnableAutoShield.SettingChanged += OnEnabledChanged;
        FeatureInitialized = EnableAutoShield.Value;
    }

    private void OnEnabledChanged(object sender, EventArgs args) => SetEnabledStatus();

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

    public InventoryGrid GetActiveInstance()
    {
        return _activeInstance;
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

    public void OnMiddleClick(UIInputHandler middleClick)
    {
        if (!FeatureInitialized || Player.m_localPlayer == null || _activeInstance == null)
            return;

        if (middleClick == null || middleClick.gameObject == null) return;

        var player = Player.m_localPlayer;

        var buttonPos = _activeInstance.GetButtonPos(middleClick.gameObject);
        ShieldMeBruhReforged.Log.LogDebug($"Button Pressed on {buttonPos.x},{buttonPos.y}");

        var itemAt = _activeInstance.m_inventory.GetItemAt(buttonPos.x, buttonPos.y);

        if (itemAt == null) return;

        ShieldMeBruhReforged.Log.LogDebug($"Item Name {itemAt.m_shared.m_name} of type {itemAt.m_shared.m_itemType}");


        if (itemAt.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield) return;

        var targetVector = new Vector2i(buttonPos.x, buttonPos.y);
        var selectedElement = _activeInstance.GetElement(buttonPos.x, buttonPos.y, _activeInstance.m_width);

        if (CurrentElement == null)
        {
            ApplyShieldToElement(selectedElement, itemAt, true);
        }
        else if (CurrentElement.Position == targetVector)
        {
            ResetCurrentSheildElement();
        }
        else if (CurrentElement.Position != targetVector)
        {
            var oldShield = _activeInstance.GetInventory().GetItemAt(CurrentElement.Position.x, CurrentElement.Position.y);
            var newShield = itemAt;

            ResetCurrentSheildElement();
            ApplyShieldToElement(selectedElement, itemAt, true);

            if (oldShield != null && oldShield.m_equipped) player.EquipItem(newShield);
        }
    }

    public void SetShieldStatus(bool statusSetTo)
    {
        FeatureInitialized = EnableAutoShield.Value;
        
        if (EnableAutoShield.Value)
        {
            if (statusSetTo)
            {
                if (Player.m_localPlayer is { } player && CurrentElement != null && SelectedShield != null)
                {
                    //Validate Location and Item
                    var itemAt = player.GetInventory().GetItemAt(CurrentElement.Position.x, CurrentElement.Position.y);

                    if (itemAt != SelectedShield)
                        statusSetTo = false;
                }
            }
            if (CurrentElement != null)
            {
                GetShield(CurrentElement).enabled = statusSetTo;
            }
        }
    }
    
    public void SetEnabledStatus()
    {
        FeatureInitialized = EnableAutoShield.Value;
        
        if (EnableAutoShield.Value)
        {
            if (CurrentElement != null)
            {
                GetShield(CurrentElement).enabled = true;
            }
            return;
        }

        if (CurrentElement != null)
            GetShield(CurrentElement).enabled = false;
    }

    public void ResetCurrentSheildElement(InventoryElement selectedElement = null)
    {
        if (CurrentElement != null && selectedElement == null) GetShield(CurrentElement).enabled = false;

        if (selectedElement != null)
            GetShield(selectedElement).enabled = false;

        CurrentElement = null;
        SelectedShield = null;
        SaveShieldSelection();
    }

    public void ApplyShieldToElement(InventoryElement selectedElement, ItemDrop.ItemData itemAt, bool allowReset = false)
    {
        if (itemAt.m_shared.m_itemType != ItemDrop.ItemData.ItemType.Shield)
            return;
        
        var img = GetShield(selectedElement);

        img.enabled = true;

        if (CurrentElement == null)
        {
            CurrentElement = selectedElement;
            SelectedShield = itemAt;
        }
        else
        {
            if ((CurrentElement.Position == selectedElement.Position && allowReset) || selectedElement.Position.x < 0 ||
                selectedElement.Position.y < 0)
            {
                GetShield(CurrentElement).enabled = false;
                CurrentElement = null;
                SelectedShield = null;
            }
            else
            {
                CurrentElement = selectedElement;
                SelectedShield = itemAt;
            }
        }

        SaveShieldSelection();
        
        SetEnabledStatus();
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

        img.enabled = false;
        return img;
    }

    public void ResetAutoShieldOnPlayerAwake()
    {
        if (DeathEvent.DeathInProgress)
            return;
        
        ShieldMeBruhReforged.Log.LogDebug($"Resetting Player Context");
        _activeInstance = null;
        CurrentElement = null;
        SelectedShield = null;
    }

    public Vector2i? GetSavedShieldPosition()
    {
        if (Player.m_localPlayer is not { } player)
            return null;

        var inventory = player.GetInventory();
        var selection = ShieldSelection.Read(player.m_customData, ShieldMeBruhReforged.PluginId,
            inventory.GetWidth(), inventory.GetHeight());
        return selection is { } slot ? new Vector2i(slot.X, slot.Y) : null;
    }

    private void SaveShieldSelection()
    {
        if (Player.m_localPlayer is { } player)
            ShieldSelection.Save(player.m_customData, ShieldMeBruhReforged.PluginId,
                CurrentElement != null ? (CurrentElement.Position.x, CurrentElement.Position.y) : null);
    }
    
    public static class ResetEvent
    {
        public static void PerformReset(Player player)
        {
            if (Player.m_localPlayer == null)
                return;
            
            player.UnequipItem(player.m_rightItem, false);
            player.UnequipItem(player.m_leftItem, false);
            ShieldMeBruhReforged.AutoShield.ResetAutoShieldOnPlayerAwake();
        }
    }
}
