using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class NpcShopsModel(NpcShopRepository npcShopRepository, RecipeRepository recipeRepository, LootTableRepository lootTableRepository) : PageModel
{
    public IEnumerable<NpcShop> Shops { get; set; } = [];
    public IEnumerable<Recipe> AllRecipes { get; set; } = [];
    public IEnumerable<LootTable> AllLootTables { get; set; } = [];
    public IReadOnlyDictionary<long, Recipe> RecipesById { get; set; } = new Dictionary<long, Recipe>();

    [BindProperty] public string? NewShopName { get; set; }
    [BindProperty] public string? NewShopLootTableId { get; set; }
    [BindProperty] public long TargetShopId { get; set; }
    [BindProperty] public long AddRecipeId { get; set; }
    [BindProperty] public string? EditLootTableId { get; set; }

    public async Task OnGetAsync()
    {
        Shops = await npcShopRepository.GetAllAsync();
        AllRecipes = await recipeRepository.GetAllAsync();
        AllLootTables = await lootTableRepository.GetAllAsync();
        RecipesById = AllRecipes.ToDictionary(r => r.Id);
    }

    public async Task<IActionResult> OnPostAddShopAsync()
    {
        await npcShopRepository.AddAsync(NewShopLootTableId, NewShopName);
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
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveRecipeAsync(long shopId, long recipeId)
    {
        await npcShopRepository.RemoveRecipeAsync(shopId, recipeId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostUpdateLootTableAsync()
    {
        ModelState.Clear();
        await npcShopRepository.UpdateLootTableAsync(TargetShopId, EditLootTableId);
        return RedirectToPage();
    }

    public List<long> ParseRecipeIds(string? json) => NpcShopRepository.ParseRecipeIds(json);
}
