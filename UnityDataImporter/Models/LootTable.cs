namespace UnityDataImporter.Models;

public class LootTable
{
    public long Id { get; set; }
    public string LootTableDatas { get; set; } = string.Empty;
    public string? LootTableName { get; set; }

    public ICollection<LootTableData> Entries { get; set; } = [];
}
