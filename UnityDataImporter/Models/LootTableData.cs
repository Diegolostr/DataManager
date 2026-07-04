namespace UnityDataImporter.Models;

public class LootTableData
{
    public long Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public int? Probability { get; set; }
    public int? MinAmount { get; set; }
    public int? MaxAmount { get; set; }
    public long? LootTableId { get; set; }

    public Item? Item { get; set; }
    public LootTable? LootTable { get; set; }
}
