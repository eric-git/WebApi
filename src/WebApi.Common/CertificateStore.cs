using System.Security.Cryptography.X509Certificates;

namespace WebApi.Common;

using static Constants;

public static class CertificateStore
{
    private static readonly Lock Sync = new();
    private static bool _loaded;

    public static IReadOnlyList<X509Certificate2>? Roots { get; private set; }

    public static void Load()
    {
        lock (Sync)
        {
            if (_loaded)
            {
                return;
            }

            Roots = Directory.EnumerateFiles(CertificateStoreRootPath, "*.crt")
                .Select(X509CertificateLoader.LoadCertificateFromFile)
                .ToList();

            _loaded = true;
        }
    }
}