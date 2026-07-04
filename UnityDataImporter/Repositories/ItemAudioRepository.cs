using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;

namespace UnityDataImporter.Repositories;

public class ItemAudioRepository(AppDbContext db)
{
    public async Task<ItemAudio> UpsertAsync(long? existingId, byte[] audioBytes)
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
