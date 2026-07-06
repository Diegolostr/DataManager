using System.Text;

var path = @"c:\Users\dlopezst\source\repos\UnityDataImporter\UnityDataImporter\Pages\Items.cshtml";
var content = File.ReadAllText(path, Encoding.UTF8);

// Fix search input: remove oninput, add onkeydown for Enter, add value binding
var old1 = "placeholder=\"Buscar por ID, nombre o descripcion...\" oninput=\"applyFilters()\"";
var new1 = "placeholder=\"Buscar por ID, nombre o descripcion... (Enter)\" value=\"@Model.SearchQuery\" onkeydown=\"if(event.key==='Enter')searchItems()\"";

if (content.Contains(old1))
{
    content = content.Replace(old1, new1);
    Console.WriteLine("Fixed search input");
}
else
{
    Console.WriteLine("Could not find search input pattern");
    // Debug: find what's actually there
    var idx = content.IndexOf("itemSearch");
    if (idx > 0)
    {
        var segment = content.Substring(idx, Math.Min(200, content.Length - idx));
        Console.WriteLine("Found: " + segment);
    }
}

File.WriteAllText(path, content, new UTF8Encoding(false));
Console.WriteLine("Done");
