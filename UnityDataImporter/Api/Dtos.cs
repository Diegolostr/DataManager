namespace UnityDataImporter.Api;

public record WeaponDataDto(long Id, long? Damage, float? Heaviness, string? Ammo, float? Cooldown);

public record Vector2Dto(double X, double? Y);

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
    ItemAudioDto? ParryAudio,
    Vector2Dto? Size);

public record LootTableEntryDto(long Id, string ItemId, int? Probability, int? MinAmount, int? MaxAmount);

public record LootTableDto(long Id, string? Name, IEnumerable<LootTableEntryDto> Entries);

public record RecipeDto(long Id, string Name, IEnumerable<CreateRecipeInputItemDto> InputItems, IEnumerable<string> OutputItems, int? RecipeCost);

public record NpcShopDto(long Id, IEnumerable<RecipeDto> Recipes, string? LootTableId, string? LootTableName);

public record CreateLootTableEntryDto(string ItemId, int? Probability, int? MinAmount, int? MaxAmount);
public record CreateLootTableDto(string Name, IEnumerable<CreateLootTableEntryDto>? Entries);

public record CreateRecipeInputItemDto(string ItemId, int Amount);
public record CreateRecipeDto(string Name, int? RecipeCost, IEnumerable<CreateRecipeInputItemDto>? InputItems, IEnumerable<string>? OutputItems);

public record CreateNpcShopDto(string? LootTableId, IEnumerable<CreateRecipeDto>? Recipes);

// ── Create DTOs (no auto-generated IDs) ────────────────────────────────────
public record CreateStatDto(string Stat, int? Amount);
public record CreateEventDto(string EventTypeId);
public record CreateWeaponDto(long? Damage, float? Heaviness, string? Ammo, float? Cooldown);
public record CreateMagicAttackDto(
    string MagicType, int? MagicDamage, float? Cooldown,
    float? ProjectileSpeed, string? EffectType, int? ManaConsumption, int? MaxCompanions);
public record CreateItemAudioDto(string Audio, string? Name, string? Prefix);

public record CreateItemDto(
    string Id,
    string? Name,
    string Description,
    bool? IsStackable,
    int? MaxAmount,
    string? Icon,
    int? BuyAmount,
    int? SellAmount,
    bool? CanBlock,
    float? BlockAmount,
    string? ItemRarity,
    string? ItemType,
    IEnumerable<CreateStatDto>? ItemStats,
    IEnumerable<CreateEventDto>? ItemEvents,
    string? EquipmentSlot,
    string? HoldType,
    CreateWeaponDto? WeaponData,
    IEnumerable<CreateMagicAttackDto>? MagicAttacks,
    CreateItemAudioDto? BlockSounds,
    CreateItemAudioDto? ParryAudio,
    Vector2Dto? Size);
