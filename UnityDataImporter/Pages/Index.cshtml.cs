using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public record LootTablePreview(long Id, int EntryCount, string? Name);
public record RecipePreview(string Name, int InputCount, int OutputCount);

public record NpcShopPreview(long Id, int RecipeCount, string? LootTableName);

public class IndexModel(ItemRepository itemRepository, LootTableRepository lootTableRepository, RecipeRepository recipeRepository, NpcShopRepository npcShopRepository, AppDbContext db) : PageModel
{
    public IEnumerable<Item> Items { get; set; } = [];
    public IEnumerable<string> ItemTypes { get; set; } = [];
    public IEnumerable<string> EquipmentSlots { get; set; } = [];
    public IEnumerable<string> HoldTypes { get; set; } = [];
    public IEnumerable<string> Rarities { get; set; } = [];
    public IEnumerable<string> StatsList { get; set; } = [];
    public IEnumerable<InventorySound> InventorySounds { get; set; } = [];
    public IEnumerable<string> EffectTypes { get; set; } = [];
    public IEnumerable<string> MagicTypes { get; set; } = [];
    public IEnumerable<string> MagicStatusEffects { get; set; } = [];
    public IEnumerable<EventType> EventTypes { get; set; } = [];
    public int LootTableCount { get; set; }
    public IEnumerable<LootTablePreview> LootTablePreviews { get; set; } = [];
    public int RecipeCount { get; set; }
    public IEnumerable<RecipePreview> RecipePreviews { get; set; } = [];
    public int NpcShopCount { get; set; }
    public IEnumerable<NpcShopPreview> NpcShopPreviews { get; set; } = [];

    [BindProperty] public string NewItemType { get; set; } = string.Empty;
    [BindProperty] public string NewEquipmentSlot { get; set; } = string.Empty;
    [BindProperty] public string NewRarity { get; set; } = string.Empty;
    [BindProperty] public string NewStat { get; set; } = string.Empty;
    [BindProperty] public string NewInventorySound { get; set; } = string.Empty;
    [BindProperty] public string NewEffectType { get; set; } = string.Empty;
    [BindProperty] public string NewMagicType { get; set; } = string.Empty;
    [BindProperty] public string NewMagicStatusEffect { get; set; } = string.Empty;
    [BindProperty] public string NewEventType { get; set; } = string.Empty;
    [BindProperty] public string NewEventTypeName { get; set; } = string.Empty;
    [BindProperty] public IFormFile? NewEventTypeIcon { get; set; }
    [BindProperty] public string EditId { get; set; } = string.Empty;
    [BindProperty] public string EditValue { get; set; } = string.Empty;

    public async Task OnGetAsync()
    {
        await LoadAllAsync();
    }

    public async Task<IActionResult> OnPostAddItemTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewItemType))
        {
            db.ItemType.Add(new ItemType { Id = NewItemType });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddEquipmentSlotAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewEquipmentSlot))
        {
            db.EquipmentSlotType.Add(new EquipmentSlotType { Id = NewEquipmentSlot });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddRarityAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewRarity))
        {
            db.Rarity.Add(new Rarity { Id = NewRarity });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddStatAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewStat))
        {
            db.Stats.Add(new Stats { Id = NewStat });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddInventorySoundAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewInventorySound))
        {
            db.InventorySound.Add(new InventorySound { Sound = NewInventorySound });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddEffectTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewEffectType))
        {
            db.EffectType.Add(new EffectType { Id = NewEffectType });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddMagicTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewMagicType))
        {
            db.MagicType.Add(new MagicType { Id = NewMagicType });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddMagicStatusEffectAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewMagicStatusEffect))
        {
            db.MagicStatusEffect.Add(new MagicStatusEffect { Id = NewMagicStatusEffect });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddEventTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewEventType))
        {
            byte[]? iconBytes = null;
            if (NewEventTypeIcon is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await NewEventTypeIcon.CopyToAsync(ms);
                iconBytes = ms.ToArray();
            }
            db.EventType.Add(new EventType
            {
                Id = NewEventType,
                EventName = string.IsNullOrWhiteSpace(NewEventTypeName) ? null : NewEventTypeName,
                Icon = iconBytes
            });
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditItemTypeAsync()
    {
        var entity = await db.ItemType.FindAsync(EditId);
        if (entity is not null) { db.ItemType.Remove(entity); db.ItemType.Add(new ItemType { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditEquipmentSlotAsync()
    {
        var entity = await db.EquipmentSlotType.FindAsync(EditId);
        if (entity is not null) { db.EquipmentSlotType.Remove(entity); db.EquipmentSlotType.Add(new EquipmentSlotType { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditRarityAsync()
    {
        var entity = await db.Rarity.FindAsync(EditId);
        if (entity is not null) { db.Rarity.Remove(entity); db.Rarity.Add(new Rarity { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditStatAsync()
    {
        var entity = await db.Stats.FindAsync(EditId);
        if (entity is not null) { db.Stats.Remove(entity); db.Stats.Add(new Stats { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditEffectTypeAsync()
    {
        var entity = await db.EffectType.FindAsync(EditId);
        if (entity is not null) { db.EffectType.Remove(entity); db.EffectType.Add(new EffectType { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditMagicTypeAsync()
    {
        var entity = await db.MagicType.FindAsync(EditId);
        if (entity is not null) { db.MagicType.Remove(entity); db.MagicType.Add(new MagicType { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditMagicStatusEffectAsync()
    {
        var entity = await db.MagicStatusEffect.FindAsync(EditId);
        if (entity is not null) { db.MagicStatusEffect.Remove(entity); db.MagicStatusEffect.Add(new MagicStatusEffect { Id = EditValue }); await db.SaveChangesAsync(); }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostEditEventTypeAsync()
    {
        var entity = await db.EventType.FindAsync(EditId);
        if (entity is not null)
        {
            if (!string.IsNullOrWhiteSpace(EditValue)) entity.EventName = EditValue;
            if (NewEventTypeIcon is { Length: > 0 })
            {
                using var ms = new MemoryStream();
                await NewEventTypeIcon.CopyToAsync(ms);
                entity.Icon = ms.ToArray();
            }
            await db.SaveChangesAsync();
        }
        return RedirectToPage();
    }

    private async Task LoadAllAsync()
    {
        Items = await itemRepository.GetAllAsync();
        ItemTypes = await db.ItemType.Select(t => t.Id).ToListAsync();
        EquipmentSlots = await db.EquipmentSlotType.Select(e => e.Id).ToListAsync();
        HoldTypes = await db.HoldType.Select(h => h.Id).ToListAsync();
        Rarities = await db.Rarity.Select(r => r.Id).ToListAsync();
        StatsList = await db.Stats.Select(s => s.Id).ToListAsync();
        InventorySounds = await db.InventorySound.ToListAsync();
        EffectTypes = await db.EffectType.Select(e => e.Id).ToListAsync();
        MagicTypes = await db.MagicType.Select(m => m.Id).ToListAsync();
        MagicStatusEffects = await db.MagicStatusEffect.Select(m => m.Id).ToListAsync();
        EventTypes = await db.EventType.ToListAsync();
        var tables = await lootTableRepository.GetAllAsync();
        LootTableCount = tables.Count();
        LootTablePreviews = tables.Select(t => new LootTablePreview(t.Id, t.Entries.Count, t.LootTableName)).ToList();
        var recipes = await recipeRepository.GetAllAsync();
        RecipeCount = recipes.Count();
        RecipePreviews = recipes.Select(r => new RecipePreview(
            r.RecipeName,
            RecipeRepository.ParseInputItems(r.InputItems).Count,
            RecipeRepository.ParseOutputItems(r.OutputItems).Count
        )).ToList();
        var shops = await npcShopRepository.GetAllAsync();
        NpcShopCount = shops.Count();
        NpcShopPreviews = shops.Select(s => new NpcShopPreview(
            s.Id,
            NpcShopRepository.ParseRecipeIds(s.Recipes).Count,
            s.LootTable?.LootTableName
        )).ToList();
    }
}
