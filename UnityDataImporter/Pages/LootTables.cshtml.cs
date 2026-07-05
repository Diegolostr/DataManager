using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class LootTablesModel(LootTableRepository lootTableRepository, ItemRepository itemRepository) : PageModel
{
    public IEnumerable<LootTable> LootTables { get; set; } = [];
    public Dictionary<string, IEnumerable<LootTableData>> EntriesByTable { get; set; } = [];
    public IEnumerable<Item> AllItems { get; set; } = [];

    [BindProperty] public LootTableData NewEntry { get; set; } = new();
    [BindProperty] public string? NewTableName { get; set; }

    public async Task OnGetAsync()
    {
        LootTables = await lootTableRepository.GetAllAsync();
        AllItems = await itemRepository.GetAllAsync();
        var dict = new Dictionary<string, IEnumerable<LootTableData>>();
        foreach (var t in LootTables)
            dict[t.LootTableName ?? ""] = await lootTableRepository.GetEntriesAsync(t.LootTableName ?? "");
        EntriesByTable = dict;
    }

    public async Task<IActionResult> OnPostAddTableAsync()
    {
        await lootTableRepository.AddAsync(NewTableName);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteTableAsync(long id)
    {
        await lootTableRepository.DeleteAsync(id);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostAddEntryAsync()
    {
        ModelState.Clear();
        await lootTableRepository.AddEntryAsync(NewEntry);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteEntryAsync(long id)
    {
        await lootTableRepository.DeleteEntryAsync(id);
        return RedirectToPage();
    }
}
