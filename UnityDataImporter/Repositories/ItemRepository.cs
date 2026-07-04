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

    public async Task AddItemAsync(Item item, byte[]? blockSoundBytes, byte[]? parryAudioBytes)
    {
        await HandleAudios(item, null, blockSoundBytes, parryAudioBytes);
        db.Items.Add(item);
        await db.SaveChangesAsync();
    }

    public async Task UpdateItemAsync(Item item, byte[]? blockSoundBytes, byte[]? parryAudioBytes)
    {
        var existing = await db.Items.AsNoTracking().FirstOrDefaultAsync(i => i.Id == item.Id);
        await HandleAudios(item, existing, blockSoundBytes, parryAudioBytes);
        db.Items.Update(item);
        await db.SaveChangesAsync();
    }

    private async Task HandleAudios(Item item, Item? existing, byte[]? blockSoundBytes, byte[]? parryAudioBytes)
    {
        if (blockSoundBytes is { Length: > 0 })
        {
            var audio = await UpsertAudio(existing?.BlockSounds, blockSoundBytes);
            item.BlockSounds = audio.Id;
        }

        if (parryAudioBytes is { Length: > 0 })
        {
            var audio = await UpsertAudio(existing?.ParryAudio, parryAudioBytes);
            item.ParryAudio = audio.Id;
        }
    }

    private async Task<ItemAudio> UpsertAudio(long? existingId, byte[] audioBytes)
    {
        if (existingId.HasValue)
        {
            var existing = await db.ItemAudio.FindAsync(existingId.Value);
            if (existing is not null)
            {
                existing.Audio = audioBytes;
                await db.SaveChangesAsync();
                return existing;
            }
        }

        var newAudio = new ItemAudio { Audio = audioBytes };
        db.ItemAudio.Add(newAudio);
        await db.SaveChangesAsync();
        return newAudio;
    }
}
