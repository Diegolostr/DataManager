namespace UnityDataImporter.Models;

public class User
{
    public long Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string? Password { get; set; }
}
