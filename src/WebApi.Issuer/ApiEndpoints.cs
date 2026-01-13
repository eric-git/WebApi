using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.DataAccess;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Issuer;

public static class ApiEndpoints
{
    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapIssuer(string issuerBaseUrl, int tokenLifeTimeInMinutes)
        {
            var uri = new Uri(issuerBaseUrl);
            var path = uri.AbsolutePath.Trim('/');
            endpointRouteBuilder.MapPost($"{path}/token", async (HttpRequest httpRequest, IConfiguration configuration, ISettingsDataRepository settingsDataRepository) =>
            {
                var formData = await httpRequest.ReadFormAsync();
                var clientId = Guid.Parse(formData["client_id"].ToString());
                var clientAssertion = formData["client_assertion"].ToString();
                var scope = formData["scope"].ToString();
                JsonWebTokenHandler jsonWebTokenHandler = new();
                var jsonWebToken = jsonWebTokenHandler.ReadJsonWebToken(clientAssertion);
                var keyId = Guid.Parse(jsonWebToken.Kid);
                var apiId = Guid.Parse(jsonWebToken.Audiences.FirstOrDefault()!);
                var publicSigningKey = await settingsDataRepository.GetSigningKeyByClientIdAsync(clientId, apiId, keyId);
                if (string.IsNullOrWhiteSpace(publicSigningKey))
                {
                    return Results.Unauthorized();
                }

                var (_, rsaPublicSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(publicSigningKey, jsonWebToken.Kid);
                var result = await jsonWebTokenHandler.ValidateTokenAsync(
                    jsonWebToken,
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

                var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var allowed = await settingsDataRepository.VerifyClientAccessAsync(clientId, apiId, requestedScopes);
                if (!allowed)
                {
                    return Results.Unauthorized();
                }

                var privateSigningKey = configuration["private-signing-key.pem"];
                var (_, rsaPrivateSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(privateSigningKey!);
                var now = DateTime.UtcNow;
                SecurityTokenDescriptor securityTokenDescriptor = new()
                {
                    Issuer = issuerBaseUrl,
                    Audience = apiId.ToString("D"),
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

            endpointRouteBuilder.MapGet($"{path}/.well-known/jwks.json", async (IConfiguration configuration) =>
            {
                var publicSigningKey = configuration["public-signing-key.pem"];
                var (rsa, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(publicSigningKey!);
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