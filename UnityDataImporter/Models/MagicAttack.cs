namespace UnityDataImporter.Models;

public class MagicAttack
{
    public long Id { get; set; }
    public string? ItemId { get; set; }
    public string MagicType { get; set; } = string.Empty;
    public int? MagicDamage { get; set; }
    public float? Cooldown { get; set; }
    public float? ProjectileSpeed { get; set; }
    public string? EffectType { get; set; }
    public string? HitSounds { get; set; }
    public int? ManaConsumption { get; set; }
    public int? MaxCompanions { get; set; }

    public Item? Item { get; set; }
    public MagicType? MagicTypeNavigation { get; set; }
    public EffectType? EffectTypeNavigation { get; set; }
}
