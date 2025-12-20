namespace WebApi.Common;

public static class TypeExtensions
{
    public static bool GuidsEqual(string? a, string? b)
    {
        return Guid.TryParse(a, out var g1) &&
               Guid.TryParse(b, out var g2) &&
               g1 == g2;
    }
}