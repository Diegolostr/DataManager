namespace UnityDataImporter.Models;

public class Item
{
    public string Id { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool? IsStackable { get; set; }
    public int? MaxAmount { get; set; }
    public byte[]? Icon { get; set; }
    public long? WeaponData { get; set; }
    public long? ItemSize { get; set; }
    public int? BuyAmount { get; set; }
    public int? SellAmount { get; set; }
    public bool? CanBlock { get; set; }
    public float? BlockAmount { get; set; }
    public long? BlockSounds { get; set; }
    public long? ParryAudio { get; set; }
    public string? ItemRarity { get; set; }
    public string? ItemType { get; set; }
    public string? ItemStats { get; set; }
    public string? ItemEvents { get; set; }
    public string? EquipmentSlot { get; set; }
    public long? ItemSoundType { get; set; }
    public string? HoldType { get; set; }

    public WeaponData? Weapon { get; set; }
    public Vector2? Size { get; set; }
    public ItemAudio? BlockSoundsNavigation { get; set; }
    public ItemAudio? ParryAudioNavigation { get; set; }
    public Rarity? Rarity { get; set; }
    public ItemType? Type { get; set; }
    public EquipmentSlotType? EquipmentSlotNavigation { get; set; }
    public InventorySound? SoundType { get; set; }
    public HoldType? HoldTypeNavigation { get; set; }
}
