using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Common;

public static class SecurityExtensions
{
    public static bool CertificateValidationCallback(HttpRequestMessage httpRequestMessage, X509Certificate2? x509Certificate2, X509Chain? x509Chain, SslPolicyErrors sslPolicyErrors)
    {
        if (sslPolicyErrors is SslPolicyErrors.None)
        {
            return true;
        }

        if (x509Certificate2 is null)
        {
            return false;
        }

        X509Chain chain = new()
        {
            ChainPolicy =
            {
                RevocationMode = X509RevocationMode.NoCheck,
                VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority,
                TrustMode = X509ChainTrustMode.CustomRootTrust
            }
        };
        foreach (var root in CertificateStore.Roots!)
        {
            chain.ChainPolicy.CustomTrustStore.Add(root);
        }

        var result = chain.Build(x509Certificate2);
        return result;
    }


    public static async Task<(RSA Rsa, RsaSecurityKey RsaSecurityKey)> CreateRsaSecurityKeyFromPemFileAsync(string pemFilePath)
    {
        var pem = await File.ReadAllTextAsync(pemFilePath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        var rsaParameters = rsa.ExportParameters(false);
        var kid = Base64UrlEncoder.Encode(SHA256.HashData(rsaParameters.Modulus!));
        return (rsa, new RsaSecurityKey(rsa) { KeyId = kid });
    }
}