using System.Reflection;

namespace BraidKit.Core;

public static class Extensions
{
    public static string ReadResourceFile(this Assembly assembly, string filename)
    {
        var qualifiedFilename = assembly.GetManifestResourceNames().Single(x => x.EndsWith(filename, StringComparison.OrdinalIgnoreCase));
        using var stream = assembly.GetManifestResourceStream(qualifiedFilename)!;
        using var reader = new StreamReader(stream);
        var result = reader.ReadToEnd();
        return result;
    }

    public static Entity GetTim(this IEnumerable<Entity> entities) => entities.FirstOrDefault(x => x.EntityType == EntityType.Guy) ?? throw new Exception("Tim not found");
}
