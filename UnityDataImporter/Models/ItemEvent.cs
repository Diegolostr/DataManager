namespace UnityDataImporter.Models;

public class ItemEvent
{
    public long Id { get; set; }
    public string ItemId { get; set; } = string.Empty;
    public string? EventName { get; set; }
    public byte[]? Icon { get; set; }
    public string? EventType { get; set; }

    public Item? Item { get; set; }
    public EventType? EventTypeNavigation { get; set; }
}
