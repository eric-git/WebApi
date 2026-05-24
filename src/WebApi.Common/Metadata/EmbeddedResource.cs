using System.Reflection;

namespace WebApi.Common.Metadata;

public static class EmbeddedResource
{
    private static readonly Assembly Assembly = Assembly.GetEntryAssembly()!;

    public static string Read(string resourceName)
    {
        var fullName = Assembly.GetManifestResourceNames()
                               .First(n => n.EndsWith(resourceName, StringComparison.OrdinalIgnoreCase));
        using var stream = Assembly.GetManifestResourceStream(fullName)!;
        using StreamReader reader = new(stream);
        return reader.ReadToEnd();
    }
}