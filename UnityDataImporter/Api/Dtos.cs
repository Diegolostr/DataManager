namespace UnityDataImporter.Api;

public record WeaponDataDto(long Id, long? Damage, int? Heaviness, string? Ammo, float? Cooldown);

public record ItemAudioDto(long Id, string Audio, string? Name, string? Prefix);

public record ItemEventDto(long Id, string? EventTypeId, string? EventName, string? EventIcon);

public record StatAmountDto(long Id, string Stat, int? Amount);

public record MagicAttackDto(
    long Id, string MagicType, int? MagicDamage, float? Cooldown,
    float? ProjectileSpeed, string? EffectType, int? ManaConsumption, int? MaxCompanions);

public record ItemDto(
    string Id,
    string? Name,
    string? Description,
    bool? IsStackable,
    int? MaxAmount,
    string? Icon,
    int? BuyAmount,
    int? SellAmount,
    bool? CanBlock,
    float? BlockAmount,
    string? ItemRarity,
    string? ItemType,
    IEnumerable<StatAmountDto> ItemStats,
    IEnumerable<ItemEventDto> ItemEvents,
    string? EquipmentSlot,
    string? HoldType,
    WeaponDataDto? WeaponData,
    IEnumerable<MagicAttackDto> MagicAttacks,
    ItemAudioDto? BlockSounds,
    ItemAudioDto? ParryAudio);

public record LootTableEntryDto(long Id, string ItemId, int? Probability, int? MinAmount, int? MaxAmount);

public record LootTableDto(long Id, string? Name, IEnumerable<LootTableEntryDto> Entries);

public record RecipeDto(long Id, string Name, string? InputItems, string? OutputItems, int? RecipeCost);

public record NpcShopDto(long Id, string Recipes, long? LootTableId, string? LootTableName);
