using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;
using UnityDataImporter.Hubs;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class RecipesModel(RecipeRepository recipeRepository, ItemRepository itemRepository, IHubContext<DataHub> hub) : PageModel
{
    public IEnumerable<Recipe> Recipes { get; set; } = [];
    public IEnumerable<Item> AllItems { get; set; } = [];
    public IReadOnlyDictionary<string, Item> ItemsById { get; set; } = new Dictionary<string, Item>();
    public long? FilteredRecipeId { get; set; }
    public long? ReturnShopId { get; set; }
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public const int PageSize = 10;

    [BindProperty] public string NewRecipeName { get; set; } = string.Empty;
    [BindProperty] public int? NewRecipeCost { get; set; }
    [BindProperty] public long TargetRecipeId { get; set; }
    [BindProperty] public string InputItemId { get; set; } = string.Empty;
    [BindProperty] public int InputItemAmount { get; set; } = 1;
    [BindProperty] public string OutputItemId { get; set; } = string.Empty;
    [BindProperty] public string EditRecipeName { get; set; } = string.Empty;
    [BindProperty] public int? EditRecipeCost { get; set; }
    [BindProperty] public List<RecipeInputItemEdit> EditInputItems { get; set; } = [];
    [BindProperty] public long? ReturnShopIdPost { get; set; }

    private IActionResult RedirectBack() =>
        ReturnShopIdPost.HasValue ? RedirectToPage("/NpcShops") : RedirectToPage();

    public async Task OnGetAsync(long? recipeId, long? returnShopId, int p = 1)
    {
        FilteredRecipeId = recipeId;
        ReturnShopId = returnShopId;
        var all = (await recipeRepository.GetAllAsync()).ToList();
        if (recipeId.HasValue)
        {
            Recipes = all.Where(r => r.Id == recipeId.Value);
            TotalPages = 1;
            CurrentPage = 1;
        }
        else
        {
            TotalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
            CurrentPage = Math.Clamp(p, 1, Math.Max(1, TotalPages));
            Recipes = all.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        }
        AllItems = await itemRepository.GetAllAsync();
        ItemsById = AllItems.ToDictionary(i => i.Id);
    }

    public async Task<IActionResult> OnPostAddRecipeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewRecipeName))
        {
            var r = await recipeRepository.AddAsync(NewRecipeName, NewRecipeCost);
            await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", r.RecipeName);
        }
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostSaveRecipeAsync(long id)
    {
        ModelState.Clear();
        var items = EditInputItems.Select(e => new RecipeInputItem(e.ItemId, e.Amount)).ToList();
        await recipeRepository.UpdateAsync(id, EditRecipeName, EditRecipeCost, items);
        await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", EditRecipeName);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostDeleteRecipeAsync(long id)
    {
        await recipeRepository.DeleteAsync(id);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddInputItemAsync()
    {
        ModelState.Clear();
        await recipeRepository.AddInputItemAsync(TargetRecipeId, InputItemId, InputItemAmount);
        var r = await recipeRepository.GetByIdAsync(TargetRecipeId);
        await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", r?.RecipeName);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostRemoveInputItemAsync(long recipeId, string itemId)
    {
        await recipeRepository.RemoveInputItemAsync(recipeId, itemId);
        var r = await recipeRepository.GetByIdAsync(recipeId);
        await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", r?.RecipeName);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostAddOutputItemAsync()
    {
        ModelState.Clear();
        await recipeRepository.AddOutputItemAsync(TargetRecipeId, OutputItemId);
        var r = await recipeRepository.GetByIdAsync(TargetRecipeId);
        await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", r?.RecipeName);
        return RedirectBack();
    }

    public async Task<IActionResult> OnPostRemoveOutputItemAsync(long recipeId, string itemId)
    {
        await recipeRepository.RemoveOutputItemAsync(recipeId, itemId);
        var r = await recipeRepository.GetByIdAsync(recipeId);
        await hub.Clients.All.SendAsync("EntityUpdated", "Recipe", r?.RecipeName);
        return RedirectBack();
    }

    public List<RecipeInputItem> ParseInputItems(string? json) => RecipeRepository.ParseInputItems(json);
    public List<string> ParseOutputItems(string? json) => RecipeRepository.ParseOutputItems(json);
}
