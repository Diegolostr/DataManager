namespace UnityDataImporter.Models;

public class NpcShop
{
    public long Id { get; set; }
    public string Recipes { get; set; } = "[]";
    public string? LootTableId { get; set; }
    public string? Name { get; set; }

    public LootTable? LootTable { get; set; }
}
