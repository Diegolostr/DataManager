namespace UnityDataImporter.Models;

public class StatsAmount
{
    public long Id { get; set; }
    public string Stat { get; set; } = string.Empty;
    public string? Item { get; set; }
    public int? Amount { get; set; }

    public Stats? StatNavigation { get; set; }
    public Item? ItemNavigation { get; set; }
}
