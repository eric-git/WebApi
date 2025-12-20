using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.Model;
using static WebApi.Common.SecurityExtensions;
using static WebApi.Common.TypeExtensions;

namespace WebApi.Issuer;

public static class ApiEndpoints
{
    private const string DataFilePath = "./data/db.json";

    private static readonly JsonSerializerOptions DataSerializationOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private static async Task<Settings> GetSettingsDataAsync()
    {
        await using var fileStream = File.OpenRead(DataFilePath);
        var settingsData = await JsonSerializer.DeserializeAsync<Settings>(fileStream, DataSerializationOptions);
        return settingsData ?? new Settings();
    }


    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapIssuer(string issuerBaseUrl, int tokenLifeTimeInMinutes)
        {
            var uri = new Uri(issuerBaseUrl);
            var path = uri.AbsolutePath.Trim('/');
            endpointRouteBuilder.MapPost($"{path}/token", async (HttpRequest httpRequest) =>
            {
                var formData = await httpRequest.ReadFormAsync();
                var clientId = formData["client_id"].ToString();
                var clientAssertion = formData["client_assertion"].ToString();
                var scope = formData["scope"].ToString();
                var (_, rsaPublicSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync($"./signing/{clientId}-public.pem");
                JwtSecurityTokenHandler handler = new();
                ClaimsPrincipal claimsPrincipal;
                try
                {
                    claimsPrincipal = handler.ValidateToken(clientAssertion, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaPublicSecurityKey,
                        ValidateIssuer = true,
                        ValidIssuer = issuerBaseUrl,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    }, out _);
                }
                catch
                {
                    return Results.Unauthorized();
                }

                var apiId = claimsPrincipal.FindFirstValue(JwtRegisteredClaimNames.Aud);
                var settings = await GetSettingsDataAsync();
                var result = settings.Clients!
                    .Any(c => GuidsEqual(c.Id, clientId) && c.Services!.Any(s => GuidsEqual(s.Id, apiId) && s.Scopes!.Contains(scope)));
                if (!result)
                {
                    return Results.Unauthorized();
                }

                var (_, rsaPrivateSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync("./signing/private.pem");
                var now = DateTime.UtcNow;
                Claim[] claims =
                [
                    new(JwtRegisteredClaimNames.Sub, clientId),
                    new("scope", scope),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
                ];
                SigningCredentials signingCredentials = new(rsaPrivateSecurityKey, SecurityAlgorithms.RsaSha256);
                JwtSecurityToken jwtSecurityToken = new(
                    issuerBaseUrl,
                    apiId,
                    claims,
                    now,
                    now.AddMinutes(tokenLifeTimeInMinutes),
                    signingCredentials);
                return Results.Json(new
                {
                    token_type = "Bearer",
                    expires_in = tokenLifeTimeInMinutes * 60,
                    access_token = handler.WriteToken(jwtSecurityToken)
                });
            });
            endpointRouteBuilder.MapGet($"{path}/.well-known/jwks.json", async () =>
            {
                var (rsa, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync("./signing/public.pem");
                var parameters = rsa.ExportParameters(false);
                var jwk = new
                {
                    kty = "RSA",
                    use = "sig",
                    kid = rsaSecurityKey.KeyId,
                    e = Base64UrlEncoder.Encode(parameters.Exponent),
                    n = Base64UrlEncoder.Encode(parameters.Modulus)
                };
                return Results.Json(new
                {
                    keys = new[] { jwk }
                }, DataSerializationOptions);
            });
            endpointRouteBuilder.MapGet($"{path}/.well-known/openid-configuration", () =>
            {
                var discoveryDoc = new
                {
                    issuer = issuerBaseUrl,
                    token_endpoint = $"{issuerBaseUrl}/token",
                    jwks_uri = $"{issuerBaseUrl}/.well-known/jwks.json",
                    id_token_signing_alg_values_supported = (string[])["RS256"],
                    response_types_supported = (string[])["client_credentials"],
                    grant_types_supported = (string[])["client_credentials"],
                    claims_supported = (string[])["sub", "iss", "aud", "exp", "iat", "name", "email"]
                };
                return Results.Json(discoveryDoc, DataSerializationOptions);
            });
            return endpointRouteBuilder;
        }
    }
}