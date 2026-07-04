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

        return Ok(items.Select(i => MapItem(i, magicByItem.GetValueOrDefault(i.Id) ?? [])));
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
        return Ok(MapItem(item, magicAttacks));
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

    private static ItemDto MapItem(Models.Item i, IEnumerable<Models.MagicAttack> magicAttacks) => new(
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
        i.ItemStats,
        i.ItemEvents,
        i.EquipmentSlot,
        i.HoldType,
        i.Weapon is null ? null : new WeaponDataDto(i.Weapon.Id, i.Weapon.Damage, i.Weapon.Heaviness, i.Weapon.Ammo, i.Weapon.Cooldown),
        magicAttacks.Select(m => new MagicAttackDto(m.Id, m.MagicType, m.MagicDamage, m.Cooldown, m.ProjectileSpeed, m.EffectType, m.ManaConsumption, m.MaxCompanions))
    );
}
