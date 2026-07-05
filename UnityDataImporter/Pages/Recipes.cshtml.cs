using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Text.Json;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class RecipesModel(RecipeRepository recipeRepository, ItemRepository itemRepository) : PageModel
{
    public IEnumerable<Recipe> Recipes { get; set; } = [];
    public IEnumerable<Item> AllItems { get; set; } = [];
    public IReadOnlyDictionary<string, Item> ItemsById { get; set; } = new Dictionary<string, Item>();

    [BindProperty] public string NewRecipeName { get; set; } = string.Empty;
    [BindProperty] public int? NewRecipeCost { get; set; }
    [BindProperty] public long TargetRecipeId { get; set; }
    [BindProperty] public string InputItemId { get; set; } = string.Empty;
    [BindProperty] public int InputItemAmount { get; set; } = 1;
    [BindProperty] public string OutputItemId { get; set; } = string.Empty;
    [BindProperty] public string EditRecipeName { get; set; } = string.Empty;
    [BindProperty] public int? EditRecipeCost { get; set; }
    [BindProperty] public List<RecipeInputItemEdit> EditInputItems { get; set; } = [];

    public async Task OnGetAsync()
    {
        Recipes = await recipeRepository.GetAllAsync();
        AllItems = await itemRepository.GetAllAsync();
        ItemsById = AllItems.ToDictionary(i => i.Id);
    }

    public async Task<IActionResult> OnPostAddRecipeAsync()
    {
        if (!string.IsNullOrWhiteSpace(NewRecipeName))
            await recipeRepository.AddAsync(NewRecipeName, NewRecipeCost);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveRecipeAsync(long id)
    {
        ModelState.Clear();
        var items = EditInputItems.Select(e => new RecipeInputItem(e.ItemId, e.Amount)).ToList();
        await recipeRepository.UpdateAsync(id, EditRecipeName, EditRecipeCost, items);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteRecipeAsync(long id)
    {
        await recipeRepository.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddInputItemAsync()
    {
        ModelState.Clear();
        await recipeRepository.AddInputItemAsync(TargetRecipeId, InputItemId, InputItemAmount);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveInputItemAsync(long recipeId, string itemId)
    {
        await recipeRepository.RemoveInputItemAsync(recipeId, itemId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddOutputItemAsync()
    {
        ModelState.Clear();
        await recipeRepository.AddOutputItemAsync(TargetRecipeId, OutputItemId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostRemoveOutputItemAsync(long recipeId, string itemId)
    {
        await recipeRepository.RemoveOutputItemAsync(recipeId, itemId);
        return RedirectToPage();
    }

    public List<RecipeInputItem> ParseInputItems(string? json) => RecipeRepository.ParseInputItems(json);
    public List<string> ParseOutputItems(string? json) => RecipeRepository.ParseOutputItems(json);
}
