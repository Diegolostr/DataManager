using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;

namespace UnityDataImporter.Repositories;

public class ItemRepository(AppDbContext db)
{
    public async Task<IEnumerable<Item>> GetAllAsync()
    {
        return await db.Items
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
    }

    public async Task<Item?> PostImageAsync(string itemId, byte[] imageBytes)
    {
        var item = await db.Items.FindAsync(itemId);
        if (item is null) return null;
        item.Icon = imageBytes;
        await db.SaveChangesAsync();
        return item;
    }

    public async Task AddItemAsync(Item item, byte[]? blockSoundBytes, byte[]? parryAudioBytes, string? blockSoundName, string? parryAudioName)
    {
        await HandleAudios(item, null, blockSoundBytes, parryAudioBytes, blockSoundName, parryAudioName);
        var size = new Vector2 { X = 1, Y = 1 };
        db.Vector2.Add(size);
        await db.SaveChangesAsync();
        item.ItemSize = size.Id;
        db.Items.Add(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item, byte[]? blockSoundBytes, byte[]? parryAudioBytes, string? blockSoundName, string? parryAudioName)
    {
        var existing = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
        await HandleAudios(item, existing, blockSoundBytes, parryAudioBytes, blockSoundName, parryAudioName);
        if (item.Icon is null && existing?.Icon is not null)
            item.Icon = existing.Icon;
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(string itemId)
    {
        // Read FK ids before deleting
        var item = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == itemId);
        var sizeId = item?.ItemSize;
        var blockSoundsId = item?.BlockSounds;
        var parryAudioId = item?.ParryAudio;

        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "magicAttacks" WHERE "itemId" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "Stats_Amount" WHERE "item" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "itemEvent" WHERE "itemId" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "lootTableData" WHERE "itemId" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "SoundsArray" WHERE "itemId" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "WeaponData" WHERE "itemId" = {0}""", itemId);
        // Nullify ammo references from other weapons pointing to this item
        await db.Database.ExecuteSqlRawAsync(
            """UPDATE "WeaponData" SET "ammo" = NULL WHERE "ammo" = {0}""", itemId);
        await db.Database.ExecuteSqlRawAsync(
            """DELETE FROM "Items" WHERE "Id" = {0}""", itemId);

        // Delete orphaned FK records
        if (sizeId.HasValue)
            await db.Database.ExecuteSqlRawAsync(
                """DELETE FROM "Vector2" WHERE "id" = {0}""", sizeId.Value);
        if (blockSoundsId.HasValue)
            await db.Database.ExecuteSqlRawAsync(
                """DELETE FROM "itemAudio" WHERE "id" = {0}""", blockSoundsId.Value);
        if (parryAudioId.HasValue)
            await db.Database.ExecuteSqlRawAsync(
                """DELETE FROM "itemAudio" WHERE "id" = {0}""", parryAudioId.Value);
    }

    private async Task HandleAudios(Item item, Item? existing, byte[]? blockSoundBytes, byte[]? parryAudioBytes, string? blockSoundName, string? parryAudioName)
    {
        if (blockSoundBytes is { Length: > 0 })
        {
            var audio = await UpsertAudio(existing?.BlockSounds, blockSoundBytes, blockSoundName, "Block");
            item.BlockSounds = audio.Id;
        }
        else
        {
            item.BlockSounds = existing?.BlockSounds;
        }

        if (parryAudioBytes is { Length: > 0 })
        {
            var audio = await UpsertAudio(existing?.ParryAudio, parryAudioBytes, parryAudioName, "Parry");
            item.ParryAudio = audio.Id;
        }
        else
        {
            item.ParryAudio = existing?.ParryAudio;
        }
    }

    private async Task<ItemAudio> UpsertAudio(long? existingId, byte[] audioBytes, string? name, string? prefix)
    {
        if (existingId.HasValue)
        {
            var existing = await db.ItemAudio.FindAsync(existingId.Value);
            if (existing is not null)
            {
                existing.Audio = audioBytes;
                existing.Name = name;
                existing.Prefix = prefix;
                await db.SaveChangesAsync();
                return existing;
            }
        }

        var newAudio = new ItemAudio { Audio = audioBytes, Name = name, Prefix = prefix };
        db.ItemAudio.Add(newAudio);
        await db.SaveChangesAsync();
        return newAudio;
    }
}
