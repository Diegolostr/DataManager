using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;

namespace UnityDataImporter.Repositories;

public class MagicAttackRepository(AppDbContext db)
{
    public async Task<IEnumerable<MagicAttack>> GetAllAsync()
        => await db.MagicAttacks
            .Include(m => m.MagicTypeNavigation)
            .Include(m => m.EffectTypeNavigation)
            .Include(m => m.HitSoundsNavigation)
            .ToListAsync();

    public async Task<IEnumerable<MagicAttack>> GetByItemIdAsync(string itemId)
        => await db.MagicAttacks
            .Include(m => m.MagicTypeNavigation)
            .Include(m => m.EffectTypeNavigation)
            .Include(m => m.HitSoundsNavigation)
            .Where(m => m.ItemId == itemId)
            .ToListAsync();

    public async Task<MagicAttack?> GetByIdAsync(long id)
        => await db.MagicAttacks
            .Include(m => m.MagicTypeNavigation)
            .Include(m => m.EffectTypeNavigation)
            .Include(m => m.HitSoundsNavigation)
            .FirstOrDefaultAsync(m => m.Id == id);

    public async Task AddAsync(MagicAttack magicAttack)
    {
        db.MagicAttacks.Add(magicAttack);
        await db.SaveChangesAsync();
    }

    public async Task UpdateAsync(MagicAttack magicAttack)
    {
        db.MagicAttacks.Update(magicAttack);
        await db.SaveChangesAsync();
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.MagicAttacks.FindAsync(id);
        if (entity is null) return;
        db.MagicAttacks.Remove(entity);
        await db.SaveChangesAsync();
    }
}
