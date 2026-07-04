namespace UnityDataImporter.Models;

public class SoundsArray
{
    public long Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string? Sounds { get; set; }

    public Item? Item { get; set; }
}
