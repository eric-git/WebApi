using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace WebApi.Common.Security;

public sealed class CertificateHandler : HttpClientHandler
{
    private readonly X509Certificate2Collection _trustedRoots = [];

    public CertificateHandler(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        _trustedRoots.ImportFromPem(configuration["ca-bundle.crt"]);
        ServerCertificateCustomValidationCallback = ValidateServerCertificate;
    }

    private bool ValidateServerCertificate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
    {
        if (cert is null)
        {
            return false;
        }

        using X509Certificate2 serverCert = new(cert);
        using X509Chain customChain = new();
        customChain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        customChain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        customChain.ChainPolicy.TrustMode = X509ChainTrustMode.CustomRootTrust;
        foreach (var root in _trustedRoots)
        {
            customChain.ChainPolicy.CustomTrustStore.Add(root);
        }

        return customChain.Build(serverCert);
    }
}