namespace UnityDataImporter.Models;

public class ItemAudio
{
    public long Id { get; set; }
    public byte[] Audio { get; set; } = [];
    public string? Name { get; set; }
    public string? Prefix { get; set; }
}
