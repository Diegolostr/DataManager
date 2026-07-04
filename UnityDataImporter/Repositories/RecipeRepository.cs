using Microsoft.EntityFrameworkCore;
using UnityDataImporter.Data;
using UnityDataImporter.Models;
using System.Text.Json;

namespace UnityDataImporter.Repositories;

public record RecipeInputItem(string ItemId, int Amount);

public class RecipeRepository(AppDbContext db)
{
    public async Task<IEnumerable<Recipe>> GetAllAsync() =>
        await db.Recipes.ToListAsync();

    public async Task<Recipe?> GetByIdAsync(long id) =>
        await db.Recipes.FindAsync(id);

    public async Task<Recipe> AddAsync(string name, int? cost)
    {
        var recipe = new Recipe { RecipeName = name, RecipeCost = cost, InputItems = "[]", OutputItems = "[]" };
        db.Recipes.Add(recipe);
        await db.SaveChangesAsync();
        return recipe;
    }

    public async Task DeleteAsync(long id)
    {
        var entity = await db.Recipes.FindAsync(id);
        if (entity is null) return;
        db.Recipes.Remove(entity);
        await db.SaveChangesAsync();
    }

    public async Task AddInputItemAsync(long recipeId, string itemId, int amount)
    {
        var recipe = await db.Recipes.FindAsync(recipeId);
        if (recipe is null) return;
        var items = JsonSerializer.Deserialize<List<RecipeInputItem>>(recipe.InputItems ?? "[]") ?? [];
        items.Add(new RecipeInputItem(itemId, amount));
        recipe.InputItems = JsonSerializer.Serialize(items);
        await db.SaveChangesAsync();
    }

    public async Task RemoveInputItemAsync(long recipeId, string itemId)
    {
        var recipe = await db.Recipes.FindAsync(recipeId);
        if (recipe is null) return;
        var items = JsonSerializer.Deserialize<List<RecipeInputItem>>(recipe.InputItems ?? "[]") ?? [];
        items.RemoveAll(i => i.ItemId == itemId);
        recipe.InputItems = JsonSerializer.Serialize(items);
        await db.SaveChangesAsync();
    }

    public async Task AddOutputItemAsync(long recipeId, string itemId)
    {
        var recipe = await db.Recipes.FindAsync(recipeId);
        if (recipe is null) return;
        var items = JsonSerializer.Deserialize<List<string>>(recipe.OutputItems ?? "[]") ?? [];
        if (!items.Contains(itemId)) items.Add(itemId);
        recipe.OutputItems = JsonSerializer.Serialize(items);
        await db.SaveChangesAsync();
    }

    public async Task RemoveOutputItemAsync(long recipeId, string itemId)
    {
        var recipe = await db.Recipes.FindAsync(recipeId);
        if (recipe is null) return;
        var items = JsonSerializer.Deserialize<List<string>>(recipe.OutputItems ?? "[]") ?? [];
        items.Remove(itemId);
        recipe.OutputItems = JsonSerializer.Serialize(items);
        await db.SaveChangesAsync();
    }

    public static List<RecipeInputItem> ParseInputItems(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        return JsonSerializer.Deserialize<List<RecipeInputItem>>(json) ?? [];
    }

    public static List<string> ParseOutputItems(string? json)
    {
        if (string.IsNullOrEmpty(json)) return [];
        return JsonSerializer.Deserialize<List<string>>(json) ?? [];
    }
}
