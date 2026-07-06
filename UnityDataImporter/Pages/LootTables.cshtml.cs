using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using UnityDataImporter.Hubs;
using UnityDataImporter.Models;
using UnityDataImporter.Repositories;

namespace UnityDataImporter.Pages;

public class LootTablesModel(LootTableRepository lootTableRepository, ItemRepository itemRepository, IHubContext<DataHub> hub) : PageModel
{
    public IEnumerable<LootTable> LootTables { get; set; } = [];
    public IEnumerable<Item> AllItems { get; set; } = [];
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
    public const int PageSize = 10;

    [BindProperty] public LootTableData NewEntry { get; set; } = new();
    [BindProperty] public string? NewTableName { get; set; }
    [BindProperty] public List<LootTableEntryEditDto> Entries { get; set; } = [];
    [BindProperty] public string? SaveEntriesTableId { get; set; }

    public async Task OnGetAsync(int page = 1)
    {
        var all = (await lootTableRepository.GetAllAsync()).ToList();
        TotalPages = (int)Math.Ceiling(all.Count / (double)PageSize);
        CurrentPage = Math.Clamp(page, 1, Math.Max(1, TotalPages));
        LootTables = all.Skip((CurrentPage - 1) * PageSize).Take(PageSize);
        AllItems = await itemRepository.GetAllAsync();
    }

    public async Task<IActionResult> OnPostAddTableAsync()
    {
        var t = await lootTableRepository.AddAsync(NewTableName);
        await hub.Clients.All.SendAsync("EntityUpdated", "LootTable", t.LootTableName);
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
        await hub.Clients.All.SendAsync("EntityUpdated", "LootTable", NewEntry.LootTableId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostSaveEntriesAsync()
    {
        ModelState.Clear();
        await lootTableRepository.UpdateEntriesAsync(Entries);
        await hub.Clients.All.SendAsync("EntityUpdated", "LootTable", SaveEntriesTableId);
        return RedirectToPage();
    }

    public async Task<IActionResult> OnPostDeleteEntryAsync(long id, string? lootTableId)
    {
        await lootTableRepository.DeleteEntryAsync(id);
        await hub.Clients.All.SendAsync("EntityUpdated", "LootTable", lootTableId);
        return RedirectToPage();
    }
}
