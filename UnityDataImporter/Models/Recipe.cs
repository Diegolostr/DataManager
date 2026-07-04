namespace UnityDataImporter.Models;

public class Recipe
{
    public long Id { get; set; }
    public string RecipeName { get; set; } = string.Empty;
    public string? InputItems { get; set; }
    public string? OutputItems { get; set; }
    public int? RecipeCost { get; set; }
}
