using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Api;
using UnityDataImporter.Data;
using UnityDataImporter.Utils;

namespace UnityDataImporter.Controllers;

[ApiController]
[Route("api")]
public class DataApiController(AppDbContext db) : ControllerBase
{
    // GET /api/items
    [HttpGet("items")]
    public async Task<IActionResult> GetAllItems()
    {
        var items = await db.Items
            .Include(i => i.Weapon)
            .Include(i => i.Size)
            .Include(i => i.Rarity)
            .Include(i => i.Type)
            .Include(i => i.EquipmentSlotNavigation)
            .Include(i => i.SoundType)
            .Include(i => i.HoldTypeNavigation)
            .Include(i => i.BlockSoundsNavigation)
            .Include(i => i.ParryAudioNavigation)
            .ToListAsync();

        var magicAttacks = await db.MagicAttacks.ToListAsync();
        var magicByItem = magicAttacks.GroupBy(m => m.ItemId).ToDictionary(g => g.Key!, g => g.ToList());

        var itemEvents = await db.ItemEvent.Include(e => e.EventType).ToListAsync();
        var eventsByItem = itemEvents.GroupBy(e => e.ItemId).ToDictionary(g => g.Key, g => g.ToList());

        var stats = await db.StatsAmount.ToListAsync();
        var statsByItem = stats.GroupBy(s => s.Item).ToDictionary(g => g.Key!, g => g.ToList());

        return Ok(items.Select(i => MapItem(i, magicByItem.GetValueOrDefault(i.Id) ?? [], eventsByItem.GetValueOrDefault(i.Id) ?? [], statsByItem.GetValueOrDefault(i.Id) ?? [])));
    }

    // GET /api/items/{id}
    [HttpGet("items/{id}")]
    public async Task<IActionResult> GetItem(string id)
    {
        var item = await db.Items
            .Include(i => i.Weapon)
            .Include(i => i.Size)
            .Include(i => i.Rarity)
            .Include(i => i.Type)
            .Include(i => i.EquipmentSlotNavigation)
            .Include(i => i.SoundType)
            .Include(i => i.HoldTypeNavigation)
            .Include(i => i.BlockSoundsNavigation)
            .Include(i => i.ParryAudioNavigation)
            .FirstOrDefaultAsync(i => i.Id == id);

        if (item is null) return NotFound();

        var magicAttacks = await db.MagicAttacks.Where(m => m.ItemId == id).ToListAsync();
        var itemEvents = await db.ItemEvent.Include(e => e.EventType).Where(e => e.ItemId == id).ToListAsync();
        var stats = await db.StatsAmount.Where(s => s.Item == id).ToListAsync();
        return Ok(MapItem(item, magicAttacks, itemEvents, stats));
    }

    // GET /api/recipes
    [HttpGet("recipes")]
    public async Task<IActionResult> GetAllRecipes()
    {
        var recipes = await db.Recipes.ToListAsync();
        return Ok(recipes.Select(r => new RecipeDto(r.Id, r.RecipeName, r.InputItems, r.OutputItems, r.RecipeCost)));
    }

    // GET /api/loottables
    [HttpGet("loottables")]
    public async Task<IActionResult> GetAllLootTables()
    {
        var tables = await db.LootTables.Include(l => l.Entries).ToListAsync();
        return Ok(tables.Select(t => new LootTableDto(
            t.Id,
            t.LootTableName,
            t.Entries.Select(e => new LootTableEntryDto(e.Id, e.ItemId, e.Probability, e.MinAmount, e.MaxAmount))
        )));
    }

    // GET /api/npcshops
    [HttpGet("npcshops")]
    public async Task<IActionResult> GetAllNpcShops()
    {
        var shops = await db.NpcsShop.Include(s => s.LootTable).ToListAsync();
        return Ok(shops.Select(s => new NpcShopDto(s.Id, s.Recipes, s.LootTableId, s.LootTable?.LootTableName)));
    }

    // POST /api/items
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
    {
        if (await db.Items.AnyAsync(i => i.Id == dto.Id))
            return Conflict($"Item '{dto.Id}' already exists.");

        // Size
        var sizeX = dto.Size?.X ?? 1;
        var sizeY = dto.Size?.Y ?? 1;
        var vec = new Models.Vector2 { X = sizeX, Y = sizeY };
        db.Vector2.Add(vec);
        await db.SaveChangesAsync();

        // Audio
        long? blockSoundsId = null, parryAudioId = null;
        if (dto.BlockSounds is not null)
        {
            var a = new Models.ItemAudio { Audio = Convert.FromBase64String(dto.BlockSounds.Audio), Name = dto.BlockSounds.Name, Prefix = dto.BlockSounds.Prefix };
            db.ItemAudio.Add(a); await db.SaveChangesAsync(); blockSoundsId = a.Id;
        }
        if (dto.ParryAudio is not null)
        {
            var a = new Models.ItemAudio { Audio = Convert.FromBase64String(dto.ParryAudio.Audio), Name = dto.ParryAudio.Name, Prefix = dto.ParryAudio.Prefix };
            db.ItemAudio.Add(a); await db.SaveChangesAsync(); parryAudioId = a.Id;
        }

        // Item
        var item = new Models.Item
        {
            Id = dto.Id, Name = dto.Name, Description = dto.Description,
            IsStackable = dto.IsStackable, MaxAmount = dto.MaxAmount,
            Icon = dto.Icon is not null ? Convert.FromBase64String(dto.Icon) : null,
            BuyAmount = dto.BuyAmount, SellAmount = dto.SellAmount,
            CanBlock = dto.CanBlock, BlockAmount = dto.BlockAmount,
            ItemRarity = dto.ItemRarity, ItemType = dto.ItemType,
            EquipmentSlot = dto.EquipmentSlot, HoldType = dto.HoldType,
            ItemSize = vec.Id, BlockSounds = blockSoundsId, ParryAudio = parryAudioId
        };
        db.Items.Add(item);
        await db.SaveChangesAsync();

        // Stats
        foreach (var s in dto.ItemStats ?? [])
            db.StatsAmount.Add(new Models.StatsAmount { Stat = s.Stat, Item = item.Id, Amount = s.Amount });

        // Events
        foreach (var ev in dto.ItemEvents ?? [])
            db.ItemEvent.Add(new Models.ItemEvent { ItemId = item.Id, EventTypeId = ev.EventTypeId });

        await db.SaveChangesAsync();

        // Weapon
        if (dto.WeaponData is not null)
        {
            db.WeaponData.Add(new Models.WeaponData { ItemId = item.Id, Damage = dto.WeaponData.Damage, Heaviness = dto.WeaponData.Heaviness, Ammo = dto.WeaponData.Ammo, Cooldown = dto.WeaponData.Cooldown });
            await db.SaveChangesAsync();
        }

        // Magic attacks
        foreach (var m in dto.MagicAttacks ?? [])
            db.MagicAttacks.Add(new Models.MagicAttack { ItemId = item.Id, MagicType = m.MagicType, MagicDamage = m.MagicDamage, Cooldown = m.Cooldown, ProjectileSpeed = m.ProjectileSpeed, EffectType = m.EffectType, ManaConsumption = m.ManaConsumption, MaxCompanions = m.MaxCompanions });

        await db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, null);
    }

    private static ItemDto MapItem(Models.Item i, IEnumerable<Models.MagicAttack> magicAttacks, IEnumerable<Models.ItemEvent> itemEvents, IEnumerable<Models.StatsAmount> stats) => new(
        i.Id,
        i.Name,
        i.Description,
        i.IsStackable,
        i.MaxAmount,
        i.Icon is not null ? ImageUtils.ToBase64(i.Icon) : null,
        i.BuyAmount,
        i.SellAmount,
        i.CanBlock,
        i.BlockAmount,
        i.ItemRarity,
        i.ItemType,
        stats.Select(s => new StatAmountDto(s.Id, s.Stat, s.Amount)),
        itemEvents.Select(e => new ItemEventDto(e.Id, e.EventTypeId, e.EventType?.EventName, e.EventType?.Icon is not null ? Convert.ToBase64String(e.EventType.Icon) : null)),
        i.EquipmentSlot,
        i.HoldType,
        i.Weapon is null ? null : new WeaponDataDto(i.Weapon.Id, i.Weapon.Damage, i.Weapon.Heaviness, i.Weapon.Ammo, i.Weapon.Cooldown),
        magicAttacks.Select(m => new MagicAttackDto(m.Id, m.MagicType, m.MagicDamage, m.Cooldown, m.ProjectileSpeed, m.EffectType, m.ManaConsumption, m.MaxCompanions)),
        i.BlockSoundsNavigation is null ? null : new ItemAudioDto(i.BlockSoundsNavigation.Id, Convert.ToBase64String(i.BlockSoundsNavigation.Audio), i.BlockSoundsNavigation.Name, i.BlockSoundsNavigation.Prefix),
        i.ParryAudioNavigation is null ? null : new ItemAudioDto(i.ParryAudioNavigation.Id, Convert.ToBase64String(i.ParryAudioNavigation.Audio), i.ParryAudioNavigation.Name, i.ParryAudioNavigation.Prefix),
        i.Size is null ? null : new Vector2Dto(i.Size.X, i.Size.Y)
    );
}
