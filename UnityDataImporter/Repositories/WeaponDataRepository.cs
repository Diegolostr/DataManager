using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;

namespace UnityDataImporter.Repositories;

public class WeaponDataRepository(AppDbContext db)
{
    public async Task<IEnumerable<WeaponData>> GetAllAsync()
    {
        return await db.WeaponData.Include(w => w.Item).ToListAsync();
    }

    public async Task<WeaponData?> GetByIdAsync(long id)
    {
        return await db.WeaponData.Include(w => w.Item).FirstOrDefaultAsync(w => w.Id == id);
    }

    public async Task AddAsync(WeaponData weaponData)
    {
        db.WeaponData.Add(weaponData);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(WeaponData weaponData)
    {
        db.WeaponData.Update(weaponData);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.WeaponData.FindAsync(id);
        if (entity is null) return;
        db.WeaponData.Remove(entity);
        await db.SaveChangesAsync();
    }
}
