using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Models;

namespace UnityDataImporter.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users { get; set; }
    public DbSet<Item> Items { get; set; }
    public DbSet<WeaponData> WeaponData { get; set; }
    public DbSet<ItemAudio> ItemAudio { get; set; }
    public DbSet<InventorySound> InventorySound { get; set; }
    public DbSet<ItemType> ItemType { get; set; }
    public DbSet<EquipmentSlotType> EquipmentSlotType { get; set; }
    public DbSet<HoldType> HoldType { get; set; }
    public DbSet<Rarity> Rarity { get; set; }
    public DbSet<Stats> Stats { get; set; }
    public DbSet<StatsAmount> StatsAmount { get; set; }
    public DbSet<Models.Vector2> Vector2 { get; set; }
    public DbSet<EffectType> EffectType { get; set; }
    public DbSet<MagicType> MagicType { get; set; }
    public DbSet<MagicStatusEffect> MagicStatusEffect { get; set; }
    public DbSet<MagicAttack> MagicAttacks { get; set; }
    public DbSet<SoundsArray> SoundsArray { get; set; }
    public DbSet<EventType> EventType { get; set; }
    public DbSet<ItemEvent> ItemEvent { get; set; }
    public DbSet<LootTable> LootTables { get; set; }
    public DbSet<LootTableData> LootTableData { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<NpcShop> NpcsShop { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("users");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Username).HasColumnName("username");
            e.Property(x => x.Password).HasColumnName("password");
        });

        modelBuilder.Entity<Item>(e =>
        {
            e.ToTable("Items");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("Id");
            e.Property(x => x.Description).HasColumnName("description");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.IsStackable).HasColumnName("isStackable");
            e.Property(x => x.MaxAmount).HasColumnName("maxAmount");
            e.Property(x => x.Icon).HasColumnName("icon");
            e.Property(x => x.ItemSize).HasColumnName("itemSize");
            e.Property(x => x.BuyAmount).HasColumnName("buyAmount");
            e.Property(x => x.SellAmount).HasColumnName("sellAmount");
            e.Property(x => x.CanBlock).HasColumnName("canBlock");
            e.Property(x => x.BlockAmount).HasColumnName("blockAmount");
            e.Property(x => x.BlockSounds).HasColumnName("blockSounds");
            e.Property(x => x.ParryAudio).HasColumnName("parryAudio");
            e.Property(x => x.ItemRarity).HasColumnName("itemRarity");
            e.Property(x => x.ItemType).HasColumnName("itemType");
            e.Property(x => x.ItemStats).HasColumnName("itemStats").HasColumnType("jsonb");
            e.Property(x => x.ItemEvents).HasColumnName("itemEvents").HasColumnType("jsonb");
            e.Property(x => x.EquipmentSlot).HasColumnName("equipmentSlot");
            e.Property(x => x.ItemSoundType).HasColumnName("itemSoundType");
            e.Property(x => x.HoldType).HasColumnName("holdType");

            e.HasOne(x => x.Weapon).WithOne(w => w.Item).HasForeignKey<WeaponData>(w => w.ItemId).IsRequired(false);
            e.HasOne(x => x.Size).WithMany().HasForeignKey(x => x.ItemSize);
            e.HasOne(x => x.BlockSoundsNavigation).WithMany().HasForeignKey(x => x.BlockSounds);
            e.HasOne(x => x.ParryAudioNavigation).WithMany().HasForeignKey(x => x.ParryAudio);
            e.HasOne(x => x.Rarity).WithMany().HasForeignKey(x => x.ItemRarity);
            e.HasOne(x => x.Type).WithMany().HasForeignKey(x => x.ItemType);
            e.HasOne(x => x.EquipmentSlotNavigation).WithMany().HasForeignKey(x => x.EquipmentSlot);
            e.HasOne(x => x.SoundType).WithMany().HasForeignKey(x => x.ItemSoundType);
            e.HasOne(x => x.HoldTypeNavigation).WithMany().HasForeignKey(x => x.HoldType);
        });

        modelBuilder.Entity<WeaponData>(e =>
        {
            e.ToTable("WeaponData");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("itemId");
            e.Property(x => x.Damage).HasColumnName("damage");
            e.Property(x => x.Heaviness).HasColumnName("heaviness");
            e.Property(x => x.Ammo).HasColumnName("ammo");
            e.Property(x => x.Cooldown).HasColumnName("cooldown");
            e.HasOne(x => x.AmmoItem).WithMany().HasForeignKey(x => x.Ammo).IsRequired(false);
        });

        modelBuilder.Entity<ItemAudio>(e =>
        {
            e.ToTable("itemAudio");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Audio).HasColumnName("audio");
            e.Property(x => x.Name).HasColumnName("name");
            e.Property(x => x.Prefix).HasColumnName("prefix");
        });

        modelBuilder.Entity<InventorySound>(e =>
        {
            e.ToTable("InventorySound");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Sound).HasColumnName("sound");
        });

        modelBuilder.Entity<ItemType>(e =>
        {
            e.ToTable("ItemType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<EquipmentSlotType>(e =>
        {
            e.ToTable("EquipmentSlotType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<HoldType>(e =>
        {
            e.ToTable("HoldType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Rarity>(e =>
        {
            e.ToTable("Rarity");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<Stats>(e =>
        {
            e.ToTable("Stats");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<StatsAmount>(e =>
        {
            e.ToTable("Stats_Amount");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Stat).HasColumnName("stat");
            e.Property(x => x.Item).HasColumnName("item");
            e.Property(x => x.Amount).HasColumnName("amount");
            e.HasOne(x => x.StatNavigation).WithMany().HasForeignKey(x => x.Stat);
            e.HasOne(x => x.ItemNavigation).WithMany().HasForeignKey(x => x.Item).IsRequired(false);
        });

        modelBuilder.Entity<Models.Vector2>(e =>
        {
            e.ToTable("Vector2");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.X).HasColumnName("x");
            e.Property(x => x.Y).HasColumnName("y");
        });

        modelBuilder.Entity<EffectType>(e =>
        {
            e.ToTable("EffectType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<MagicType>(e =>
        {
            e.ToTable("magicType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<MagicStatusEffect>(e =>
        {
            e.ToTable("magicStatusEffect");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
        });

        modelBuilder.Entity<SoundsArray>(e =>
        {
            e.ToTable("SoundsArray");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("itemId");
            e.Property(x => x.Sounds).HasColumnName("sounds").HasColumnType("json");
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId);
        });

        modelBuilder.Entity<EventType>(e =>
        {
            e.ToTable("eventType");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Icon).HasColumnName("icon");
            e.Property(x => x.EventName).HasColumnName("eventName");
        });

        modelBuilder.Entity<ItemEvent>(e =>
        {
            e.ToTable("itemEvent");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("itemId");
            e.Property(x => x.EventTypeId).HasColumnName("eventTypeId");
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId);
            e.HasOne(x => x.EventType).WithMany().HasForeignKey(x => x.EventTypeId).IsRequired(false);
        });

        modelBuilder.Entity<LootTable>(e =>
        {
            e.ToTable("LootTables");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.LootTableDatas).HasColumnName("lootTableDatas").HasColumnType("jsonb");
            e.Property(x => x.LootTableName).HasColumnName("lootTableName");
        });

        modelBuilder.Entity<LootTableData>(e =>
        {
            e.ToTable("lootTableData");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("itemId");
            e.Property(x => x.Probability).HasColumnName("probability");
            e.Property(x => x.MinAmount).HasColumnName("minAmount");
            e.Property(x => x.MaxAmount).HasColumnName("maxAmount");
            e.Property(x => x.LootTableId).HasColumnName("lootTableId");
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId);
        });

        modelBuilder.Entity<Recipe>(e =>
        {
            e.ToTable("Recipes");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.RecipeName).HasColumnName("recipeName");
            e.Property(x => x.InputItems).HasColumnName("inputItems").HasColumnType("json");
            e.Property(x => x.OutputItems).HasColumnName("outputItems").HasColumnType("json");
            e.Property(x => x.RecipeCost).HasColumnName("recipeCost");
        });

        modelBuilder.Entity<NpcShop>(e =>
        {
            e.ToTable("NpcsShop");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.Recipes).HasColumnName("recipes").HasColumnType("jsonb");
            e.Property(x => x.LootTableId).HasColumnName("lootTableId");
            e.Property(x => x.Name).HasColumnName("name");
            e.HasOne(x => x.LootTable).WithMany().HasForeignKey(x => x.LootTableId).IsRequired(false);
        });

        modelBuilder.Entity<MagicAttack>(e =>
        {
            e.ToTable("magicAttacks");
            e.HasKey(x => x.Id);
            e.Property(x => x.Id).HasColumnName("id");
            e.Property(x => x.ItemId).HasColumnName("itemId");
            e.Property(x => x.MagicType).HasColumnName("magicType");
            e.Property(x => x.MagicDamage).HasColumnName("magicDamage");
            e.Property(x => x.Cooldown).HasColumnName("cooldown");
            e.Property(x => x.ProjectileSpeed).HasColumnName("projectileSpeed");
            e.Property(x => x.EffectType).HasColumnName("effectType");
            e.Property(x => x.HitSounds).HasColumnName("hitSounds");
            e.Property(x => x.ManaConsumption).HasColumnName("manaConsumption");
            e.Property(x => x.MaxCompanions).HasColumnName("maxCompanions");
            e.HasOne(x => x.Item).WithMany().HasForeignKey(x => x.ItemId).IsRequired(false);
            e.HasOne(x => x.HitSoundsNavigation).WithMany().HasForeignKey(x => x.HitSounds).IsRequired(false);
            e.HasOne(x => x.MagicTypeNavigation).WithMany().HasForeignKey(x => x.MagicType);
            e.HasOne(x => x.EffectTypeNavigation).WithMany().HasForeignKey(x => x.EffectType).IsRequired(false);
        });
    }
}
