using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace WebApi.Common.Security;

public static class Helper
{
    public static (RSA Rsa, RsaSecurityKey RsaSecurityKey) CreateRsaSecurityKeyFromPem(string pem, string? keyId = null)
    {
        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);
        var rsaParameters = rsa.ExportParameters(false);
        var kid = string.IsNullOrWhiteSpace(keyId) ? Base64UrlEncoder.Encode(SHA256.HashData(rsaParameters.Modulus!)) : keyId;
        return (rsa, new RsaSecurityKey(rsa) { KeyId = kid });
    }

    public static string? WrapPublicKey(string? base64)
    {
        return string.IsNullOrWhiteSpace(base64)
            ? null
            : $"""
               -----BEGIN PUBLIC KEY-----
               {base64}
               -----END PUBLIC KEY-----
               """;
    }
}