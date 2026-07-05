using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using System.Text.Json;

namespace UnityDataImporter.Repositories;

public class LootTableRepository(AppDbContext db)
{
    public async Task<IEnumerable<LootTable>> GetAllAsync() =>
        await db.LootTables.Include(l => l.Entries).ThenInclude(e => e.Item).ToListAsync();

    public async Task<IEnumerable<LootTableData>> GetEntriesAsync(string lootTableId) =>
        await db.LootTableData.Include(e => e.Item).Where(e => e.LootTableId == lootTableId).ToListAsync();

    public async Task<LootTable?> GetByIdAsync(long id) =>
        await db.LootTables.Include(l => l.Entries).ThenInclude(e => e.Item).FirstOrDefaultAsync(l => l.Id == id);

    public async Task<LootTable> AddAsync(string? name)
    {
        var lootTable = new LootTable { LootTableDatas = "[]", LootTableName = name ?? "", LootTableId = name };
        db.LootTables.Add(lootTable);
        await db.SaveChangesAsync();
        return lootTable;
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.LootTables.Include(l => l.Entries).FirstOrDefaultAsync(l => l.Id == id);
        if (entity is null) return;
        db.LootTableData.RemoveRange(entity.Entries);
        db.LootTables.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task AddEntryAsync(LootTableData entry)
    {
        db.LootTableData.Add(entry);
        await db.SaveChangesAsync();
        await SyncJsonAsync(entry.LootTableId);
    }

    public async Task DeleteEntryAsync(long entryId)
    {
        var entry = await db.LootTableData.FindAsync(entryId);
        if (entry is null) return;
        var tableId = entry.LootTableId;
        db.LootTableData.Remove(entry);
        await db.SaveChangesAsync();
        await SyncJsonAsync(tableId);
    }

    private async Task SyncJsonAsync(string? lootTableId)
    {
        if (lootTableId is null) return;
        var table = await db.LootTables.FirstOrDefaultAsync(t => t.LootTableId == lootTableId);
        if (table is null) return;
        var ids = await db.LootTableData.Where(e => e.LootTableId == lootTableId).Select(e => e.Id).ToListAsync();
        table.LootTableDatas = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }
}
