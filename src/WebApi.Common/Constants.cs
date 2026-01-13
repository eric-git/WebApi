using System.Text.Encodings.Web;
using System.Text.Json;

namespace WebApi.Common;

public static class Constants
{
    public static JsonSerializerOptions DataSerializationOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}