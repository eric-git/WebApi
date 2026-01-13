using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using Microsoft.Extensions.Configuration;

namespace WebApi.Common;

public sealed class ClientCertificateHandler : HttpClientHandler
{
    private readonly X509Certificate2Collection _trustedRoots = [];

    public ClientCertificateHandler(IConfiguration configuration)
    {
        _trustedRoots.ImportFromPem(configuration["ca-bundle.crt"]);
        ServerCertificateCustomValidationCallback = ValidateServerCertificate;
    }

    private bool ValidateServerCertificate(object sender, X509Certificate? cert, X509Chain? chain, SslPolicyErrors errors)
    {
        if (cert is null)
        {
            return false;
        }

        X509Certificate2 serverCert = new(cert);
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