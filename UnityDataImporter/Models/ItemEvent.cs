namespace UnityDataImporter.Models;

public class ItemEvent
{
    public long Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string? EventTypeId { get; set; }

    public Item? Item { get; set; }
    public EventType? EventType { get; set; }
}
