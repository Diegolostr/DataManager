using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class ItemsModel(ItemRepository itemRepository, MagicAttackRepository magicAttackRepository, AppDbContext db) : PageModel
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
    public IEnumerable<EventType> EventTypes { get; set; } = [];
    public ILookup<string, ItemEvent> ItemEventsByItem { get; set; } = Enumerable.Empty<ItemEvent>().ToLookup(e => e.ItemId);
    public ILookup<string, MagicAttack> MagicAttacksByItem { get; set; } = Enumerable.Empty<MagicAttack>().ToLookup(m => m.ItemId ?? "");
    public ILookup<string, StatsAmount> StatsByItem { get; set; } = Enumerable.Empty<StatsAmount>().ToLookup(s => s.Item ?? "");
    [BindProperty] public string? FilteredItemId { get; set; }

    private IActionResult RedirectBack() =>
        FilteredItemId is not null ? RedirectToPage(new { itemId = FilteredItemId }) : RedirectToPage();

    [BindProperty] public Item NewItem { get; set; } = new();
    [BindProperty] public IFormFile? NewBlockSound { get; set; }
    [BindProperty] public IFormFile? NewParryAudio { get; set; }
    [BindProperty] public IFormFile? EditBlockSound { get; set; }
    [BindProperty] public IFormFile? EditParryAudio { get; set; }
    [BindProperty] public IFormFile? EditIcon { get; set; }
    [BindProperty] public string NewItemTypeValue { get; set; } = string.Empty;
    [BindProperty] public string NewRarityValue { get; set; } = string.Empty;
    [BindProperty] public string NewHoldTypeValue { get; set; } = string.Empty;
    [BindProperty] public string NewEventTypeValue { get; set; } = string.Empty;
    [BindProperty] public IFormFile? NewEventTypeIcon { get; set; }

    // Pending state submitted as JSON on save
    [BindProperty] public string? PendingStatsJson { get; set; }
    [BindProperty] public string? PendingEventsJson { get; set; }
    [BindProperty] public string? PendingWeaponJson { get; set; }
    [BindProperty] public string? PendingMagicJson { get; set; }
    [BindProperty] public string? DeletedStatsJson { get; set; }
    [BindProperty] public string? DeletedEventsJson { get; set; }
    [BindProperty] public string? DeletedMagicJson { get; set; }
    [BindProperty] public string? PendingAnimationsJson { get; set; }
    [BindProperty] public string? DeleteAnimationsJson { get; set; }
    [BindProperty] public bool DeleteBlockSound { get; set; }
    [BindProperty] public bool DeleteParryAudio { get; set; }
    [BindProperty] public string? EditItemId { get; set; }

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
        var allItemEvents = await db.ItemEvent.Include(e => e.EventType).ToListAsync();
        ItemEventsByItem = allItemEvents.ToLookup(e => e.ItemId);
    }

    public async Task<IActionResult> OnPostAddItemAsync()
    {
        ModelState.Clear();
        var blockSound = await ReadFileAsync(NewBlockSound);
        var parryAudio = await ReadFileAsync(NewParryAudio);
        await itemRepository.AddItemAsync(NewItem, blockSound, parryAudio, NewBlockSound?.FileName, NewParryAudio?.FileName);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostDeleteItemAsync(string id)
    {
        try
        {
            await itemRepository.DeleteAsync(id);
        }
        catch (Exception ex)
        {
            TempData["Error"] = ex.Message;
        }
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveItemAsync()
    {
        ModelState.Clear();
        if (string.IsNullOrEmpty(EditItemId)) return RedirectBack();

        var itemId = EditItemId;

        // 1. Update item fields
        var editItem = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId);
        if (editItem is null) return RedirectBack();

        await TryBindItemFromForm(editItem);
        var blockSound = await ReadFileAsync(EditBlockSound);
        var parryAudio = await ReadFileAsync(EditParryAudio);
        await itemRepository.UpdateItemAsync(editItem, blockSound, parryAudio, EditBlockSound?.FileName, EditParryAudio?.FileName);
        var iconBytes = await ReadFileAsync(EditIcon);
        if (iconBytes is { Length: > 0 }) await itemRepository.PostImageAsync(itemId, iconBytes);

        // Size upsert
        var sizeX = double.TryParse(Request.Form["EditItem.SizeX"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var sx) ? sx : (double?)null;
        var sizeY = double.TryParse(Request.Form["EditItem.SizeY"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var sy) ? sy : (double?)null;
        if (sizeX.HasValue)
        {
            var itemForSize = await db.Items.FindAsync(itemId);
            if (itemForSize is not null)
            {
                if (itemForSize.ItemSize.HasValue)
                {
                    var vec = await db.Vector2.FindAsync(itemForSize.ItemSize.Value);
                    if (vec is not null) { vec.X = sizeX.Value; vec.Y = sizeY; }
                }
                else
                {
                    var vec = new Models.Vector2 { X = sizeX.Value, Y = sizeY };
                    db.Vector2.Add(vec);
                    await db.SaveChangesAsync();
                    itemForSize.ItemSize = vec.Id;
                }
                await db.SaveChangesAsync();
            }
        }

        // 2. Deleted stats
        var deletedStats = ParseLongList(DeletedStatsJson);
        foreach (var sid in deletedStats)
        {
            var s = await db.StatsAmount.FindAsync(sid);
            if (s is not null) db.StatsAmount.Remove(s);
        }

        // 3. New stats
        var newStats = ParseJson<List<PendingStat>>(PendingStatsJson);
        foreach (var ps in newStats)
            db.StatsAmount.Add(new StatsAmount { Stat = ps.Stat, Item = itemId, Amount = ps.Amount });

        await db.SaveChangesAsync();
        await SyncItemStatsJsonAsync(itemId);

        // 4. Deleted events
        var deletedEvents = ParseLongList(DeletedEventsJson);
        foreach (var eid in deletedEvents)
        {
            var e = await db.ItemEvent.FindAsync(eid);
            if (e is not null) db.ItemEvent.Remove(e);
        }

        // 5. New events
        var newEvents = ParseJson<List<PendingEvent>>(PendingEventsJson);
        foreach (var pe in newEvents)
            db.ItemEvent.Add(new ItemEvent { ItemId = itemId, EventTypeId = pe.EventTypeId });

        await db.SaveChangesAsync();
        await SyncItemEventsJsonAsync(itemId);

        // 6. Weapon
        var weaponData = string.IsNullOrWhiteSpace(PendingWeaponJson) ? null :
            JsonSerializer.Deserialize<PendingWeapon>(PendingWeaponJson);
        if (weaponData is not null)
        {
            var existing = await db.WeaponData.FirstOrDefaultAsync(w => w.ItemId == itemId);
            if (existing is not null)
            {
                existing.Damage = weaponData.Damage;
                existing.Heaviness = weaponData.Heaviness;
                existing.Ammo = weaponData.Ammo;
                existing.Cooldown = weaponData.Cooldown;
            }
            else
            {
                db.WeaponData.Add(new WeaponData { ItemId = itemId, Damage = weaponData.Damage, Heaviness = weaponData.Heaviness, Ammo = weaponData.Ammo, Cooldown = weaponData.Cooldown });
            }
            await db.SaveChangesAsync();
        }

        // 7. Deleted magic attacks
        var deletedMagic = ParseLongList(DeletedMagicJson);
        foreach (var mid in deletedMagic)
            await magicAttackRepository.DeleteAsync(mid);

        // 8. New/edited magic attacks
        var magicList = ParseJson<List<PendingMagic>>(PendingMagicJson);
        foreach (var pm in magicList)
        {
            if (pm.Id > 0)
            {
                var ma = await magicAttackRepository.GetByIdAsync(pm.Id);
                if (ma is not null)
                {
                    ma.MagicType = pm.MagicType; ma.MagicDamage = pm.MagicDamage; ma.Cooldown = pm.Cooldown;
                    ma.ProjectileSpeed = pm.ProjectileSpeed; ma.EffectType = pm.EffectType;
                    ma.ManaConsumption = pm.ManaConsumption; ma.MaxCompanions = pm.MaxCompanions;
                    if (pm.HitSounds is not null) ma.HitSounds = JsonSerializer.Serialize(pm.HitSounds.Select(s => Convert.FromBase64String(s.Replace("\n", "").Replace(" ", "+"))));
                    await magicAttackRepository.UpdateAsync(ma);
                }
            }
            else
            {
                await magicAttackRepository.AddAsync(new MagicAttack
                {
                    ItemId = itemId, MagicType = pm.MagicType, MagicDamage = pm.MagicDamage,
                    Cooldown = pm.Cooldown, ProjectileSpeed = pm.ProjectileSpeed, EffectType = pm.EffectType,
                    ManaConsumption = pm.ManaConsumption, MaxCompanions = pm.MaxCompanions,
                    HitSounds = pm.HitSounds is not null ? JsonSerializer.Serialize(pm.HitSounds.Select(s => Convert.FromBase64String(s.Replace("\n", "").Replace(" ", "+")))) : null
                });
            }
        }

        // 9. Animations
        var deleteAnimIndices = ParseJson<List<int>>(DeleteAnimationsJson);
        if (deleteAnimIndices.Count > 0 || !string.IsNullOrWhiteSpace(PendingAnimationsJson))
        {
            var animItem = await db.Items.FindAsync(itemId);
            if (animItem is not null)
            {
                var existing = animItem.ItemAnimations is not null
                    ? JsonSerializer.Deserialize<List<byte[]>>(animItem.ItemAnimations) ?? []
                    : new List<byte[]>();
                // remove deleted indices
                var kept = existing.Where((_, i) => !deleteAnimIndices.Contains(i)).ToList();
                // append new
                if (!string.IsNullOrWhiteSpace(PendingAnimationsJson))
                {
                    var newB64 = JsonSerializer.Deserialize<List<string>>(PendingAnimationsJson) ?? [];
                    kept.AddRange(newB64.Select(Convert.FromBase64String));
                }
                animItem.ItemAnimations = kept.Count > 0 ? JsonSerializer.Serialize(kept) : null;
                await db.SaveChangesAsync();
            }
        }

        // 10. Delete block sound / parry audio
        if (DeleteBlockSound || DeleteParryAudio)
        {
            var audioItem = await db.Items.FindAsync(itemId);
            if (audioItem is not null)
            {
                if (DeleteBlockSound && audioItem.BlockSounds.HasValue)
                {
                    var audio = await db.ItemAudio.FindAsync(audioItem.BlockSounds.Value);
                    audioItem.BlockSounds = null;
                    await db.SaveChangesAsync();
                    if (audio is not null) db.ItemAudio.Remove(audio);
                }
                if (DeleteParryAudio && audioItem.ParryAudio.HasValue)
                {
                    var audio = await db.ItemAudio.FindAsync(audioItem.ParryAudio.Value);
                    audioItem.ParryAudio = null;
                    await db.SaveChangesAsync();
                    if (audio is not null) db.ItemAudio.Remove(audio);
                }
                await db.SaveChangesAsync();
            }
        }

        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddEventTypeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewEventTypeValue))
        {
            var iconBytes = await ReadFileAsync(NewEventTypeIcon);
            db.EventType.Add(new EventType { Id = NewEventTypeValue, Icon = iconBytes });
            await db.SaveChangesAsync();
        }
        return RedirectBack();
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

    private async Task TryBindItemFromForm(Item item)
    {
        item.Name = Request.Form["EditItem.Name"];
        item.Description = Request.Form["EditItem.Description"]!;
        item.ItemType = NullIfEmpty(Request.Form["EditItem.ItemType"]);
        item.ItemRarity = NullIfEmpty(Request.Form["EditItem.ItemRarity"]);
        item.EquipmentSlot = NullIfEmpty(Request.Form["EditItem.EquipmentSlot"]);
        item.HoldType = NullIfEmpty(Request.Form["EditItem.HoldType"]);
        item.ItemSoundType = long.TryParse(Request.Form["EditItem.ItemSoundType"], out var st) ? st : null;
        item.IsStackable = bool.TryParse(Request.Form["EditItem.IsStackable"], out var isSt) ? isSt : null;
        item.MaxAmount = int.TryParse(Request.Form["EditItem.MaxAmount"], out var ma) ? ma : null;
        item.BuyAmount = int.TryParse(Request.Form["EditItem.BuyAmount"], out var ba) ? ba : null;
        item.SellAmount = int.TryParse(Request.Form["EditItem.SellAmount"], out var sa) ? sa : null;
        item.CanBlock = bool.TryParse(Request.Form["EditItem.CanBlock"], out var cb) ? cb : null;
        item.BlockAmount = float.TryParse(Request.Form["EditItem.BlockAmount"], System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var bla) ? bla : null;
        await Task.CompletedTask;
    }

    private static string? NullIfEmpty(string? s) => string.IsNullOrWhiteSpace(s) ? null : s;

    private async Task SyncItemStatsJsonAsync(string itemId)
    {
        var item = await db.Items.FindAsync(itemId);
        if (item is null) return;
        var ids = await db.StatsAmount.Where(s => s.Item == itemId).Select(s => s.Id).ToListAsync();
        item.ItemStats = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }

    private async Task SyncItemEventsJsonAsync(string itemId)
    {
        var item = await db.Items.FindAsync(itemId);
        if (item is null) return;
        var ids = await db.ItemEvent.Where(e => e.ItemId == itemId).Select(e => e.Id).ToListAsync();
        item.ItemEvents = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
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
        EventTypes = await db.EventType.ToListAsync();
    }

    private static async Task<byte[]?> ReadFileAsync(IFormFile? file)
    {
        if (file is not { Length: > 0 }) return null;
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        return ms.ToArray();
    }

    private static List<long> ParseLongList(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try { return JsonSerializer.Deserialize<List<long>>(json) ?? []; } catch { return []; }
    }

    private static T ParseJson<T>(string? json) where T : new()
    {
        if (string.IsNullOrWhiteSpace(json)) return new T();
        var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        try { return JsonSerializer.Deserialize<T>(json, opts) ?? new T(); } catch { return new T(); }
    }

    private record PendingStat(string Stat, int? Amount);
    private record PendingEvent(string? EventTypeId);
    private record PendingWeapon(long? Damage, float? Heaviness, string? Ammo, float? Cooldown);
    private record PendingMagic(long Id, string MagicType, int? MagicDamage, float? Cooldown, float? ProjectileSpeed, string? EffectType, int? ManaConsumption, int? MaxCompanions, List<string>? HitSounds);
}
