namespace UnityDataImporter.Models;

public class NpcShop
{
    public long Id { get; set; }
    public string Recipes { get; set; } = "[]";
    public long? LootTableId { get; set; }

    public LootTable? LootTable { get; set; }
}
