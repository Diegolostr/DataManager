using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using System.Text.Json;

namespace UnityDataImporter.Repositories;

public class NpcShopRepository(AppDbContext db)
{
    public async Task<IEnumerable<NpcShop>> GetAllAsync() =>
        await db.NpcsShop.Include(n => n.LootTable).ToListAsync();

    public async Task<NpcShop> AddAsync(string? lootTableId)
    {
        var shop = new NpcShop { Recipes = "[]", LootTableId = lootTableId };
        db.NpcsShop.Add(shop);
        await db.SaveChangesAsync();
        return shop;
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.NpcsShop.FindAsync(id);
        if (entity is null) return;
        db.NpcsShop.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task AddRecipeAsync(long shopId, long recipeId)
    {
        var shop = await db.NpcsShop.FindAsync(shopId);
        if (shop is null) return;
        var ids = JsonSerializer.Deserialize<List<long>>(shop.Recipes) ?? [];
        if (!ids.Contains(recipeId)) ids.Add(recipeId);
        shop.Recipes = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }

    public async Task RemoveRecipeAsync(long shopId, long recipeId)
    {
        var shop = await db.NpcsShop.FindAsync(shopId);
        if (shop is null) return;
        var ids = JsonSerializer.Deserialize<List<long>>(shop.Recipes) ?? [];
        ids.Remove(recipeId);
        shop.Recipes = JsonSerializer.Serialize(ids);
        await db.SaveChangesAsync();
    }

    public async Task UpdateLootTableAsync(long shopId, string? lootTableId)
    {
        var shop = await db.NpcsShop.FindAsync(shopId);
        if (shop is null) return;
        shop.LootTableId = lootTableId;
        await db.SaveChangesAsync();
    }

    public static List<long> ParseRecipeIds(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        return JsonSerializer.Deserialize<List<long>>(json) ?? [];
    }
}
