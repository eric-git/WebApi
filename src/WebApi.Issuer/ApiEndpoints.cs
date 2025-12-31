using System.Text.Json;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.Model;
using static WebApi.Common.Constants;
using static WebApi.Common.SecurityExtensions;
using static WebApi.Common.TypeExtensions;

namespace WebApi.Issuer;

public static class ApiEndpoints
{
    private static readonly string DataFilePath = Path.Combine(DataStoreRootPath, "db.json");

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
                var publicKeyPath = Path.Combine(KeyStoreRootPath, $"{clientId}-public.pem");
                var (_, rsaPublicSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync(publicKeyPath);
                JsonWebTokenHandler jsonWebTokenHandler = new();
                var result = await jsonWebTokenHandler.ValidateTokenAsync(
                    clientAssertion,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaPublicSecurityKey,
                        ValidateIssuer = true,
                        ValidIssuer = issuerBaseUrl,
                        ValidateAudience = false,
                        ValidateLifetime = true
                    });
                if (!result.IsValid)
                {
                    return Results.Unauthorized();
                }

                var jwt = (JsonWebToken)result.SecurityToken;
                var apiId = jwt.Audiences.FirstOrDefault();
                var settings = await GetSettingsDataAsync();
                var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var allowed = settings.Clients!
                    .Any(c => GuidsEqual(c.Id, clientId) && c.Services!.Any(s => GuidsEqual(s.Id, apiId) && requestedScopes.All(rs => s.Scopes!.Contains(rs))));
                if (!allowed)
                {
                    return Results.Unauthorized();
                }

                var privateKeyPath = Path.Combine(KeyStoreRootPath, "private.pem");
                var (_, rsaPrivateSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync(privateKeyPath);
                var now = DateTime.UtcNow;
                SecurityTokenDescriptor securityTokenDescriptor = new()
                {
                    Issuer = issuerBaseUrl,
                    Audience = apiId,
                    Claims = new Dictionary<string, object>
                    {
                        { JwtRegisteredClaimNames.Sub, clientId },
                        { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N") },
                        { "scope", requestedScopes }
                    },
                    NotBefore = now,
                    Expires = now.AddMinutes(tokenLifeTimeInMinutes),
                    SigningCredentials = new SigningCredentials(rsaPrivateSecurityKey, SecurityAlgorithms.RsaSha256)
                };
                var accessToken = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
                return Results.Json(new
                {
                    token_type = "Bearer",
                    expires_in = tokenLifeTimeInMinutes * 60,
                    access_token = accessToken
                });
            });

            endpointRouteBuilder.MapGet($"{path}/.well-known/jwks.json", async () =>
            {
                var fileName = Path.Combine(KeyStoreRootPath, "public.pem");
                var (rsa, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync(fileName);
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
                });
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
                    claims_supported = (string[])["sub", "iss", "aud", "scope", "exp", "iat", "name", "email"]
                };
                return Results.Json(discoveryDoc);
            });
            return endpointRouteBuilder;
        }
    }
}