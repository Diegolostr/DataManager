namespace UnityDataImporter.Models;

public class WeaponData
{
    public long Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public long? Damage { get; set; }
    public int? Heaviness { get; set; }
    public string? Ammo { get; set; }
    public float? Cooldown { get; set; }

    public Item? Item { get; set; }
    public Item? AmmoItem { get; set; }
}
