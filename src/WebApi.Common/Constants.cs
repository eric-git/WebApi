using System.Text.Encodings.Web;
using System.Text.Json;

namespace WebApi.Common;

public static class Constants
{
    public static string AssetsRootPath { get; } = Path.Combine(AppContext.BaseDirectory, "assets");

    public static string CertificateStoreRootPath { get; } = Path.Combine(AssetsRootPath, "https");

    public static string DataStoreRootPath { get; } = Path.Combine(AssetsRootPath, "data");

    public static string KeyStoreRootPath { get; } = Path.Combine(AssetsRootPath, "signing");

    public static JsonSerializerOptions DataSerializationOptions { get; } = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };
}