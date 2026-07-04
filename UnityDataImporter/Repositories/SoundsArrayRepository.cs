using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;

namespace UnityDataImporter.Repositories;

public class SoundsArrayRepository(AppDbContext db)
{
    public async Task<IEnumerable<SoundsArray>> GetAllAsync()
        => await db.SoundsArray.Include(s => s.Item).ToListAsync();

    public async Task<SoundsArray?> GetByIdAsync(long id)
        => await db.SoundsArray.FindAsync(id);

    public async Task<SoundsArray?> GetByItemIdAsync(string itemId)
        => await db.SoundsArray.Include(s => s.Item).FirstOrDefaultAsync(s => s.ItemId == itemId);

    public async Task AddAsync(SoundsArray soundsArray)
    {
        db.SoundsArray.Add(soundsArray);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(SoundsArray soundsArray)
    {
        db.SoundsArray.Update(soundsArray);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.SoundsArray.FindAsync(id);
        if (entity is null) return;
        db.SoundsArray.Remove(entity);
        await db.SaveChangesAsync();
    }
}
