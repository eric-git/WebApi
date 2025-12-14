using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Common;

public static class SecurityExtensions
{
    public static Func<HttpRequestMessage, X509Certificate2?, X509Chain?, SslPolicyErrors, bool> CertificateValidationCallback { get; } = (_, cert, chain, errors) =>
    {
        if (errors == SslPolicyErrors.None)
        {
            return true;
        }

        chain!.ChainPolicy.ExtraStore.Add(cert!);
        chain.ChainPolicy.VerificationFlags = X509VerificationFlags.AllowUnknownCertificateAuthority;
        chain.ChainPolicy.RevocationMode = X509RevocationMode.NoCheck;
        return chain.Build(cert!);
    };

    public static async Task<(RSA Rsa, RsaSecurityKey RsaSecurityKey)> CreateRsaSecurityKeyFromPemFile(string pemFilePath)
    {
        var pem = await File.ReadAllTextAsync(pemFilePath);
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        var rsaParameters = rsa.ExportParameters(false);
        var kid = Base64UrlEncoder.Encode(SHA256.HashData(rsaParameters.Modulus!));
        return (rsa, new RsaSecurityKey(rsa) { KeyId = kid });
    }
}