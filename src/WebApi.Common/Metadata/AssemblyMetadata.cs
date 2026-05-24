using System.Reflection;

namespace WebApi.Common.Metadata;

public static class AssemblyMetadata
{
    private static readonly Assembly Assembly = Assembly.GetEntryAssembly()!;

    public static string? Get(string key)
    {
        return Assembly.GetCustomAttributes<AssemblyMetadataAttribute>()
                       .FirstOrDefault(a => a.Key == key)
                       ?.Value;
    }

    public static string? GetTitle()
    {
        return Assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
    }

    public static string? GetProduct()
    {
        return Assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product;
    }

    public static string? GetCompany()
    {
        return Assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company;
    }

    public static string? GetInformationalVersion()
    {
        return Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
    }

    public static string? GetVersion()
    {
        return Assembly.GetName().Version?.ToString();
    }
}