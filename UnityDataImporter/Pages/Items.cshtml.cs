using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class ItemsModel(ItemRepository itemRepository, WeaponDataRepository weaponDataRepository, MagicAttackRepository magicAttackRepository, SoundsArrayRepository soundsArrayRepository, AppDbContext db) : PageModel
{
    public IEnumerable<Item> Items { get; set; } = [];
    public IEnumerable<string> Rarities { get; set; } = [];
    public IEnumerable<string> ItemTypes { get; set; } = [];
    public IEnumerable<string> EquipmentSlots { get; set; } = [];
    public IEnumerable<string> HoldTypes { get; set; } = [];
    public IEnumerable<InventorySound> InventorySounds { get; set; } = [];
    public IEnumerable<string> MagicTypes { get; set; } = [];
    public IEnumerable<string> EffectTypes { get; set; } = [];
    public IEnumerable<string> StatTypes { get; set; } = [];
    public IEnumerable<string> EventTypes { get; set; } = [];
    public ILookup<string, ItemEvent> ItemEventsByItem { get; set; } = Enumerable.Empty<ItemEvent>().ToLookup(e => e.ItemId);
    public ILookup<string, MagicAttack> MagicAttacksByItem { get; set; } = Enumerable.Empty<MagicAttack>().ToLookup(m => m.ItemId ?? "");
    public ILookup<string, StatsAmount> StatsByItem { get; set; } = Enumerable.Empty<StatsAmount>().ToLookup(s => s.Item ?? "");
    [BindProperty] public string? FilteredItemId { get; set; }

    private IActionResult RedirectBack() =>
        FilteredItemId is not null ? RedirectToPage(new { itemId = FilteredItemId }) : RedirectToPage();

    [BindProperty] public Item NewItem { get; set; } = new();
    [BindProperty] public Item EditItem { get; set; } = new();
    [BindProperty] public WeaponData EditWeapon { get; set; } = new();
    [BindProperty] public IFormFile? NewBlockSound { get; set; }
    [BindProperty] public IFormFile? NewParryAudio { get; set; }
    [BindProperty] public IFormFile? EditBlockSound { get; set; }
    [BindProperty] public IFormFile? EditParryAudio { get; set; }
    [BindProperty] public IFormFile? EditIcon { get; set; }

    [BindProperty] public MagicAttack NewMagicAttack { get; set; } = new();
    [BindProperty] public StatsAmount NewStat { get; set; } = new();

    [BindProperty] public string NewItemTypeValue { get; set; } = string.Empty;
    [BindProperty] public string NewRarityValue { get; set; } = string.Empty;
    [BindProperty] public string NewHoldTypeValue { get; set; } = string.Empty;
    [BindProperty] public string NewEventTypeValue { get; set; } = string.Empty;
    [BindProperty] public IFormFile? NewItemEventIcon { get; set; }
    [BindProperty] public ItemEvent NewItemEvent { get; set; } = new();

    public async Task OnGetAsync(string? itemId)
    {
        FilteredItemId = itemId;
        var all = await itemRepository.GetAllAsync();
        Items = itemId is not null ? all.Where(i => i.Id == itemId) : all;
        await LoadLookupsAsync();
        var allMagicAttacks = await magicAttackRepository.GetAllAsync();
        MagicAttacksByItem = allMagicAttacks.ToLookup(m => m.ItemId ?? "");
        var allStats = await db.StatsAmount.Include(s => s.StatNavigation).ToListAsync();
        StatsByItem = allStats.ToLookup(s => s.Item ?? "");
        var allItemEvents = await db.ItemEvent.Include(e => e.EventTypeNavigation).ToListAsync();
        ItemEventsByItem = allItemEvents.ToLookup(e => e.ItemId);
    }

    public async Task<IActionResult> OnPostUploadAsync(string itemId, IFormFile file)
    {
        if (file is { Length: > 0 })
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);
            await itemRepository.PostImageAsync(itemId, ms.ToArray());
        }
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddItemAsync()
    {
        ModelState.Clear();
        var blockSound = await ReadFileAsync(NewBlockSound);
        var parryAudio = await ReadFileAsync(NewParryAudio);
        await itemRepository.AddItemAsync(NewItem, blockSound, parryAudio);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostEditItemAsync()
    {
        ModelState.Clear();
        var blockSound = await ReadFileAsync(EditBlockSound);
        var parryAudio = await ReadFileAsync(EditParryAudio);
        await itemRepository.UpdateItemAsync(EditItem, blockSound, parryAudio);
        var iconBytes = await ReadFileAsync(EditIcon);
        if (iconBytes is { Length: > 0 })
            await itemRepository.PostImageAsync(EditItem.Id, iconBytes);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostEditWeaponAsync()
    {
        ModelState.Clear();
        await weaponDataRepository.UpdateAsync(EditWeapon);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddStatAsync()
    {
        ModelState.Clear();
        db.StatsAmount.Add(NewStat);
        await db.SaveChangesAsync();
        await SyncItemStatsJsonAsync(NewStat.Item!);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostDeleteStatAsync(long id)
    {
        var entity = await db.StatsAmount.FindAsync(id);
        if (entity is not null)
        {
            var itemId = entity.Item;
            db.StatsAmount.Remove(entity);
            await db.SaveChangesAsync();
            if (itemId is not null) await SyncItemStatsJsonAsync(itemId);
        }
        return RedirectBack();
    }

    private async Task SyncItemStatsJsonAsync(string itemId)
    {
        var item = await db.Items.FindAsync(itemId);
        if (item is null) return;
        var ids = await db.StatsAmount.Where(s => s.Item == itemId).Select(s => s.Id).ToListAsync();
        item.ItemStats = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }

    public async Task<IActionResult> OnPostAddHitSoundAsync(long magicAttackId, IFormFile file)
    {
        ModelState.Clear();
        if (file is not { Length: > 0 }) return RedirectBack();

        var attack = await magicAttackRepository.GetByIdAsync(magicAttackId);
        if (attack is null) return RedirectBack();

        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var audio = new ItemAudio { Audio = ms.ToArray() };
        db.ItemAudio.Add(audio);
        await db.SaveChangesAsync();

        SoundsArray soundsArray;
        if (attack.HitSounds.HasValue)
        {
            soundsArray = (await soundsArrayRepository.GetByIdAsync(attack.HitSounds.Value))!;
            var ids = JsonSerializer.Deserialize<List<long>>(soundsArray.Sounds ?? "[]") ?? [];
            ids.Add(audio.Id);
            soundsArray.Sounds = JsonSerializer.Serialize(ids);
            await soundsArrayRepository.UpdateAsync(soundsArray);
        }
        else
        {
            soundsArray = new SoundsArray { ItemId = attack.ItemId ?? string.Empty, Sounds = JsonSerializer.Serialize(new[] { audio.Id }) };
            await soundsArrayRepository.AddAsync(soundsArray);
            attack.HitSounds = soundsArray.Id;
            await magicAttackRepository.UpdateAsync(attack);
        }

        return RedirectBack();
    }

    public async Task<IActionResult> OnPostRemoveHitSoundAsync(long magicAttackId, long audioId)
    {
        var attack = await magicAttackRepository.GetByIdAsync(magicAttackId);
        if (attack?.HitSounds is null) return RedirectBack();

        var soundsArray = await soundsArrayRepository.GetByIdAsync(attack.HitSounds.Value);
        if (soundsArray is null) return RedirectBack();

        var ids = JsonSerializer.Deserialize<List<long>>(soundsArray.Sounds ?? "[]") ?? [];
        ids.Remove(audioId);
        soundsArray.Sounds = JsonSerializer.Serialize(ids);
        await soundsArrayRepository.UpdateAsync(soundsArray);

        var audioEntity = await db.ItemAudio.FindAsync(audioId);
        if (audioEntity is not null) db.ItemAudio.Remove(audioEntity);
        await db.SaveChangesAsync();

        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddMagicAttackAsync()
    {
        ModelState.Clear();
        await magicAttackRepository.AddAsync(NewMagicAttack);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostDeleteMagicAttackAsync(long id)
    {
        await magicAttackRepository.DeleteAsync(id);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostEditMagicAttackAsync()
    {
        ModelState.Clear();
        await magicAttackRepository.UpdateAsync(NewMagicAttack);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddEventTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewEventTypeValue))
        {
            db.EventType.Add(new EventType { Id = NewEventTypeValue });
            await db.SaveChangesAsync();
        }
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddItemEventAsync()
    {
        ModelState.Clear();
        var iconBytes = await ReadFileAsync(NewItemEventIcon);
        if (iconBytes is { Length: > 0 }) NewItemEvent.Icon = iconBytes;
        db.ItemEvent.Add(NewItemEvent);
        await db.SaveChangesAsync();
        await SyncItemEventsJsonAsync(NewItemEvent.ItemId);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostDeleteItemEventAsync(long id)
    {
        var entity = await db.ItemEvent.FindAsync(id);
        if (entity is not null)
        {
            var itemId = entity.ItemId;
            db.ItemEvent.Remove(entity);
            await db.SaveChangesAsync();
            await SyncItemEventsJsonAsync(itemId);
        }
        return RedirectBack();
    }

    private async Task SyncItemEventsJsonAsync(string itemId)
    {
        var item = await db.Items.FindAsync(itemId);
        if (item is null) return;
        var ids = await db.ItemEvent.Where(e => e.ItemId == itemId).Select(e => e.Id).ToListAsync();
        item.ItemEvents = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }

    public async Task<IActionResult> OnPostAddItemTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewItemTypeValue))
        {
            db.ItemType.Add(new ItemType { Id = NewItemTypeValue });
            await db.SaveChangesAsync();
        }
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddRarityAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewRarityValue))
        {
            db.Rarity.Add(new Rarity { Id = NewRarityValue });
            await db.SaveChangesAsync();
        }
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddHoldTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewHoldTypeValue))
        {
            db.HoldType.Add(new HoldType { Id = NewHoldTypeValue });
            await db.SaveChangesAsync();
        }
        return RedirectBack();
    }

    private async Task LoadLookupsAsync()
    {
        Rarities = await db.Rarity.Select(r => r.Id).ToListAsync();
        ItemTypes = await db.ItemType.Select(t => t.Id).ToListAsync();
        EquipmentSlots = await db.EquipmentSlotType.Select(e => e.Id).ToListAsync();
        HoldTypes = await db.HoldType.Select(h => h.Id).ToListAsync();
        InventorySounds = await db.InventorySound.ToListAsync();
        MagicTypes = await db.MagicType.Select(m => m.Id).ToListAsync();
        EffectTypes = await db.EffectType.Select(e => e.Id).ToListAsync();
        StatTypes = await db.Stats.Select(s => s.Id).ToListAsync();
        EventTypes = await db.EventType.Select(e => e.Id).ToListAsync();
    }

    private static async Task<byte[]?> ReadFileAsync(IFormFile? file)
    {
        if (file is not { Length: > 0 }) return null;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }
}
