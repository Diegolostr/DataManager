using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using UnityDataImporter.Api;
using UnityDataImporter.Data;
using UnityDataImporter.Utils;

namespace UnityDataImporter.Controllers;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ApiKeyAuthAttribute : Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var expectedKey = Environment.GetEnvironmentVariable("API_KEY");
        if (string.IsNullOrWhiteSpace(expectedKey)) { await next(); return; }
        if (!context.HttpContext.Request.Headers.TryGetValue("X-Api-Key", out var key) || key != expectedKey)
        {
            context.Result = new UnauthorizedResult();
            return;
        }
        await next();
    }
}

[ApiController]
[Route("api")]
[AllowAnonymous]
[ApiKeyAuth]
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
        return Ok(recipes.Select(r => new RecipeDto(
            r.Id, r.RecipeName,
            JsonSerializer.Deserialize<IEnumerable<CreateRecipeInputItemDto>>(r.InputItems ?? "[]") ?? [],
            JsonSerializer.Deserialize<IEnumerable<string>>(r.OutputItems ?? "[]") ?? [],
            r.RecipeCost)));
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

    // POST /api/loottables
    [HttpPost("loottables")]
    public async Task<IActionResult> CreateLootTable([FromBody] CreateLootTableDto dto)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var table = new Models.LootTable { LootTableName = dto.Name, LootTableDatas = "[]" };
            db.LootTables.Add(table);
            await db.SaveChangesAsync();

            foreach (var e in dto.Entries ?? [])
                db.LootTableData.Add(new Models.LootTableData { LootTableId = table.Id, ItemId = e.ItemId, Probability = e.Probability, MinAmount = e.MinAmount, MaxAmount = e.MaxAmount });
            await db.SaveChangesAsync();

            var entryIds = await db.LootTableData.Where(e => e.LootTableId == table.Id).Select(e => e.Id).ToListAsync();
            table.LootTableDatas = JsonSerializer.Serialize(entryIds);
            await db.SaveChangesAsync();

            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetAllLootTables), new { id = table.Id }, new LootTableDto(table.Id, table.LootTableName, entryIds.Select((id, i) => { var e = dto.Entries!.ElementAt(i); return new LootTableEntryDto(id, e.ItemId, e.Probability, e.MinAmount, e.MaxAmount); })));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // POST /api/recipes
    [HttpPost("recipes")]
    public async Task<IActionResult> CreateRecipe([FromBody] CreateRecipeDto dto)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var inputItems = (dto.InputItems ?? []).Select(i => new { i.ItemId, i.Amount }).ToList();
            var recipe = new Models.Recipe
            {
                RecipeName = dto.Name,
                RecipeCost = dto.RecipeCost,
                InputItems = JsonSerializer.Serialize(inputItems),
                OutputItems = JsonSerializer.Serialize(dto.OutputItems ?? [])
            };
            db.Recipes.Add(recipe);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetAllRecipes), new { id = recipe.Id }, new RecipeDto(
                recipe.Id, recipe.RecipeName,
                dto.InputItems ?? [],
                dto.OutputItems ?? [],
                recipe.RecipeCost));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // POST /api/npcshops
    [HttpPost("npcshops")]
    public async Task<IActionResult> CreateNpcShop([FromBody] CreateNpcShopDto dto)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var recipeIds = new List<long>();
            foreach (var r in dto.Recipes ?? [])
            {
                var recipe = new Models.Recipe
                {
                    RecipeName = r.Name,
                    RecipeCost = r.RecipeCost,
                    InputItems = JsonSerializer.Serialize(r.InputItems ?? []),
                    OutputItems = JsonSerializer.Serialize(r.OutputItems ?? [])
                };
                db.Recipes.Add(recipe);
                await db.SaveChangesAsync();
                recipeIds.Add(recipe.Id);
            }

            var shop = new Models.NpcShop
            {
                LootTableId = dto.LootTableId,
                Recipes = JsonSerializer.Serialize(recipeIds)
            };
            db.NpcsShop.Add(shop);
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return CreatedAtAction(nameof(GetAllNpcShops), new { id = shop.Id }, new NpcShopDto(shop.Id, shop.Recipes, shop.LootTableId, null));
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    // POST /api/items
    [HttpPost("items")]
    public async Task<IActionResult> CreateItem([FromBody] CreateItemDto dto)
    {
        await using var tx = await db.Database.BeginTransactionAsync();
        try
        {
            var existing = await db.Items.FirstOrDefaultAsync(i => i.Id == dto.Id);
            IActionResult result = existing is null ? await InsertItem(dto) : await UpdateItem(dto, existing);
            await tx.CommitAsync();
            return result;
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync();
            return BadRequest(new { error = ex.InnerException?.Message ?? ex.Message });
        }
    }

    private async Task<IActionResult> InsertItem(CreateItemDto dto)
    {
        var vec = new Models.Vector2 { X = dto.Size?.X ?? 1, Y = dto.Size?.Y ?? 1 };
        db.Vector2.Add(vec);
        await db.SaveChangesAsync();

        var (blockSoundsId, parryAudioId) = await UpsertAudios(dto, null, null);

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

        await UpsertRelations(dto, item.Id, isNew: true);
        return CreatedAtAction(nameof(GetItem), new { id = item.Id }, null);
    }

    private async Task<IActionResult> UpdateItem(CreateItemDto dto, Models.Item existing)
    {
        if (existing.ItemSize.HasValue)
        {
            var vec = await db.Vector2.FindAsync(existing.ItemSize.Value);
            if (vec is not null) { vec.X = dto.Size?.X ?? vec.X; vec.Y = dto.Size?.Y ?? vec.Y; }
        }
        else if (dto.Size is not null)
        {
            var vec = new Models.Vector2 { X = dto.Size.X, Y = dto.Size.Y };
            db.Vector2.Add(vec);
            await db.SaveChangesAsync();
            existing.ItemSize = vec.Id;
        }

        var (blockSoundsId, parryAudioId) = await UpsertAudios(dto, existing.BlockSounds, existing.ParryAudio);

        existing.Name = dto.Name;
        existing.Description = dto.Description;
        existing.IsStackable = dto.IsStackable;
        existing.MaxAmount = dto.MaxAmount;
        if (dto.Icon is not null) existing.Icon = Convert.FromBase64String(dto.Icon);
        existing.BuyAmount = dto.BuyAmount;
        existing.SellAmount = dto.SellAmount;
        existing.CanBlock = dto.CanBlock;
        existing.BlockAmount = dto.BlockAmount;
        existing.ItemRarity = dto.ItemRarity;
        existing.ItemType = dto.ItemType;
        existing.EquipmentSlot = dto.EquipmentSlot;
        existing.HoldType = dto.HoldType;
        existing.BlockSounds = blockSoundsId;
        existing.ParryAudio = parryAudioId;

        await db.SaveChangesAsync();
        await UpsertRelations(dto, existing.Id, isNew: false);
        return Ok();
    }

    private async Task UpsertRelations(CreateItemDto dto, string itemId, bool isNew)
    {
        if (!isNew)
        {
            db.StatsAmount.RemoveRange(db.StatsAmount.Where(s => s.Item == itemId));
            db.ItemEvent.RemoveRange(db.ItemEvent.Where(e => e.ItemId == itemId));
            db.MagicAttacks.RemoveRange(db.MagicAttacks.Where(m => m.ItemId == itemId));
            var existingWeapon = await db.WeaponData.FirstOrDefaultAsync(w => w.ItemId == itemId);
            if (existingWeapon is not null) db.WeaponData.Remove(existingWeapon);
            await db.SaveChangesAsync();
        }

        foreach (var s in dto.ItemStats ?? [])
            db.StatsAmount.Add(new Models.StatsAmount { Stat = s.Stat, Item = itemId, Amount = s.Amount });
        foreach (var ev in dto.ItemEvents ?? [])
            db.ItemEvent.Add(new Models.ItemEvent { ItemId = itemId, EventTypeId = ev.EventTypeId });
        await db.SaveChangesAsync();

        if (dto.WeaponData is not null)
        {
            db.WeaponData.Add(new Models.WeaponData { ItemId = itemId, Damage = dto.WeaponData.Damage, Heaviness = dto.WeaponData.Heaviness, Ammo = dto.WeaponData.Ammo, Cooldown = dto.WeaponData.Cooldown });
            await db.SaveChangesAsync();
        }

        foreach (var m in dto.MagicAttacks ?? [])
            db.MagicAttacks.Add(new Models.MagicAttack { ItemId = itemId, MagicType = m.MagicType, MagicDamage = m.MagicDamage, Cooldown = m.Cooldown, ProjectileSpeed = m.ProjectileSpeed, EffectType = m.EffectType, ManaConsumption = m.ManaConsumption, MaxCompanions = m.MaxCompanions });
        await db.SaveChangesAsync();
    }

    private async Task<(long? blockSoundsId, long? parryAudioId)> UpsertAudios(CreateItemDto dto, long? existingBlockId, long? existingParryId)
    {
        long? blockSoundsId = existingBlockId, parryAudioId = existingParryId;
        if (dto.BlockSounds is not null)
        {
            var bytes = Convert.FromBase64String(dto.BlockSounds.Audio);
            if (existingBlockId.HasValue)
            {
                var a = await db.ItemAudio.FindAsync(existingBlockId.Value);
                if (a is not null) { a.Audio = bytes; a.Name = dto.BlockSounds.Name; a.Prefix = dto.BlockSounds.Prefix; }
            }
            else
            {
                var a = new Models.ItemAudio { Audio = bytes, Name = dto.BlockSounds.Name, Prefix = dto.BlockSounds.Prefix };
                db.ItemAudio.Add(a); await db.SaveChangesAsync(); blockSoundsId = a.Id;
            }
        }
        if (dto.ParryAudio is not null)
        {
            var bytes = Convert.FromBase64String(dto.ParryAudio.Audio);
            if (existingParryId.HasValue)
            {
                var a = await db.ItemAudio.FindAsync(existingParryId.Value);
                if (a is not null) { a.Audio = bytes; a.Name = dto.ParryAudio.Name; a.Prefix = dto.ParryAudio.Prefix; }
            }
            else
            {
                var a = new Models.ItemAudio { Audio = bytes, Name = dto.ParryAudio.Name, Prefix = dto.ParryAudio.Prefix };
                db.ItemAudio.Add(a); await db.SaveChangesAsync(); parryAudioId = a.Id;
            }
        }
        await db.SaveChangesAsync();
        return (blockSoundsId, parryAudioId);
    }

    private static ItemDto MapItem(Models.Item i, IEnumerable<Models.MagicAttack> magicAttacks, IEnumerable<Models.ItemEvent> itemEvents, IEnumerable<Models.StatsAmount> stats) => new(
        i.Id, i.Name, i.Description, i.IsStackable, i.MaxAmount,
        i.Icon is not null ? ImageUtils.ToBase64(i.Icon) : null,
        i.BuyAmount, i.SellAmount, i.CanBlock, i.BlockAmount, i.ItemRarity, i.ItemType,
        stats.Select(s => new StatAmountDto(s.Id, s.Stat, s.Amount)),
        itemEvents.Select(e => new ItemEventDto(e.Id, e.EventTypeId, e.EventType?.EventName, e.EventType?.Icon is not null ? Convert.ToBase64String(e.EventType.Icon) : null)),
        i.EquipmentSlot, i.HoldType,
        i.Weapon is null ? null : new WeaponDataDto(i.Weapon.Id, i.Weapon.Damage, i.Weapon.Heaviness, i.Weapon.Ammo, i.Weapon.Cooldown),
        magicAttacks.Select(m => new MagicAttackDto(m.Id, m.MagicType, m.MagicDamage, m.Cooldown, m.ProjectileSpeed, m.EffectType, m.ManaConsumption, m.MaxCompanions)),
        i.BlockSoundsNavigation is null ? null : new ItemAudioDto(i.BlockSoundsNavigation.Id, Convert.ToBase64String(i.BlockSoundsNavigation.Audio), i.BlockSoundsNavigation.Name, i.BlockSoundsNavigation.Prefix),
        i.ParryAudioNavigation is null ? null : new ItemAudioDto(i.ParryAudioNavigation.Id, Convert.ToBase64String(i.ParryAudioNavigation.Audio), i.ParryAudioNavigation.Name, i.ParryAudioNavigation.Prefix),
        i.Size is null ? null : new Vector2Dto(i.Size.X, i.Size.Y)
    );
}
