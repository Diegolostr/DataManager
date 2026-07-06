using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using UnityDataImporter.Data;
using UnityDataImporter.Hubs;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class NpcShopsModel(NpcShopRepository npcShopRepository, RecipeRepository recipeRepository, LootTableRepository lootTableRepository, ItemRepository itemRepository, IHubContext<DataHub> hub, AppDbContext db) : PageModel
{
    public IEnumerable<NpcShop> Shops { get; set; } = [];
    public IEnumerable<Recipe> AllRecipes { get; set; } = [];
    public IEnumerable<LootTable> AllLootTables { get; set; } = [];
    public IReadOnlyDictionary<long, Recipe> RecipesById { get; set; } = new Dictionary<long, Recipe>();
    public IReadOnlyDictionary<string, Item> ItemsById { get; set; } = new Dictionary<string, Item>();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public const int PageSize = 10;

    [BindProperty] public string? NewShopName { get; set; }
    [BindProperty] public string? NewShopLootTableId { get; set; }
    [BindProperty] public long TargetShopId { get; set; }
    [BindProperty] public long AddRecipeId { get; set; }
    [BindProperty] public string? EditLootTableId { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        var all = (await npcShopRepository.GetAllAsync()).ToList();
        TotalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
        CurrentPage = Math.Clamp(page, 1, Math.Max(1, TotalPages));
        Shops = all.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        AllRecipes = await recipeRepository.GetAllAsync();
        AllLootTables = await lootTableRepository.GetAllAsync();
        RecipesById = AllRecipes.ToDictionary(r => r.Id);
        var allItems = await itemRepository.GetAllAsync();
        ItemsById = allItems.ToDictionary(i => i.Id);
    }

    public async Task<IActionResult> OnPostAddShopAsync()
    {
        var s = await npcShopRepository.AddAsync(NewShopLootTableId, NewShopName);
        await hub.Clients.All.SendAsync("EntityUpdated", "NpcShop", s.Name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteShopAsync(long id)
    {
        await npcShopRepository.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddRecipeAsync()
    {
        ModelState.Clear();
        await npcShopRepository.AddRecipeAsync(TargetShopId, AddRecipeId);
        var s = await db.NpcsShop.FindAsync(TargetShopId);
        await hub.Clients.All.SendAsync("EntityUpdated", "NpcShop", s?.Name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveRecipeAsync(long shopId, long recipeId)
    {
        await npcShopRepository.RemoveRecipeAsync(shopId, recipeId);
        var s = await db.NpcsShop.FindAsync(shopId);
        await hub.Clients.All.SendAsync("EntityUpdated", "NpcShop", s?.Name);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateLootTableAsync()
    {
        ModelState.Clear();
        await npcShopRepository.UpdateLootTableAsync(TargetShopId, EditLootTableId);
        var s = await db.NpcsShop.FindAsync(TargetShopId);
        await hub.Clients.All.SendAsync("EntityUpdated", "NpcShop", s?.Name);
        return RedirectToPage();
    }

    public List<long> ParseRecipeIds(string? json) => NpcShopRepository.ParseRecipeIds(json);
    public string? GetFirstOutputItemId(Recipe recipe) {
        var ids = RecipeRepository.ParseOutputItems(recipe.OutputItems);
        return ids.FirstOrDefault();
    }
}
