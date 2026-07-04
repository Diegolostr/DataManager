namespace UnityDataImporter.Utils;

public static class ImageUtils
{
    public static async Task<byte[]> ToByteArrayAsync(string imagePath)
    {
        return await File.ReadAllBytesAsync(imagePath);
    }

    public static string ToBase64(byte[] imageBytes)
    {
        return Convert.ToBase64String(imageBytes);
    }
}
