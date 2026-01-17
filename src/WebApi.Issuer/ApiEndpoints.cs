using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.DataAccess;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Issuer;

internal static class ApiEndpoints
{
    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapIssuer(string issuerBaseUrl, int tokenLifeTimeInMinutes)
        {
            Uri uri = new(issuerBaseUrl);
            var path = uri.AbsolutePath.Trim('/');
            endpointRouteBuilder.MapPost($"{path}/token", async (HttpRequest httpRequest, IConfiguration configuration, ISettingsDataRepository settingsDataRepository) =>
            {
                var formData = await httpRequest.ReadFormAsync().ConfigureAwait(false);
                var clientId = Guid.Parse(formData["client_id"].ToString());
                var clientAssertion = formData["client_assertion"].ToString();
                var scope = formData["scope"].ToString();
                JsonWebTokenHandler jsonWebTokenHandler = new();
                var jsonWebToken = jsonWebTokenHandler.ReadJsonWebToken(clientAssertion);
                var keyId = Guid.Parse(jsonWebToken.Kid);
                var audience = jsonWebToken.Audiences.FirstOrDefault();
                var apiId = Guid.Parse(audience!);
                var publicSigningKey = await settingsDataRepository.GetSigningKeyByClientIdAsync(clientId, apiId, keyId).ConfigureAwait(false);
                if (string.IsNullOrWhiteSpace(publicSigningKey))
                {
                    return Results.Unauthorized();
                }

                var (_, rsaPublicSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(publicSigningKey, jsonWebToken.Kid).ConfigureAwait(false);
                var result = await jsonWebTokenHandler.ValidateTokenAsync(
                    jsonWebToken,
                    new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaPublicSecurityKey,
                        ValidIssuer = issuerBaseUrl,
                        ValidAudience = audience
                    }).ConfigureAwait(false);
                if (!result.IsValid)
                {
                    return Results.Unauthorized();
                }

                var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var allowed = await settingsDataRepository.VerifyClientAccessAsync(clientId, apiId, requestedScopes).ConfigureAwait(false);
                if (!allowed)
                {
                    return Results.Unauthorized();
                }

                var client = await settingsDataRepository.GetClientDetailsById(clientId).ConfigureAwait(false);
                var privateSigningKey = configuration["private-signing-key.pem"];
                var (_, rsaPrivateSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(privateSigningKey!).ConfigureAwait(false);
                var now = DateTime.UtcNow;
                SecurityTokenDescriptor securityTokenDescriptor = new()
                {
                    Issuer = issuerBaseUrl,
                    Audience = apiId.ToString("D"),
                    Claims = new Dictionary<string, object>
                    {
                        { JwtRegisteredClaimNames.Name, client!.Name },
                        { JwtRegisteredClaimNames.Email, client.Email },
                        { JwtRegisteredClaimNames.Sub, clientId },
                        { JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N") },
                        { "scope", requestedScopes },
                        { "idp", issuerBaseUrl }
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
                var (rsa, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(publicSigningKey!).ConfigureAwait(false);
                var parameters = rsa.ExportParameters(false);
                var jwk = new
                {
                    kty = JsonWebAlgorithmsKeyTypes.RSA,
                    use = JsonWebKeyUseNames.Sig,
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
                    claims_supported = (string[])
                    [
                        JwtRegisteredClaimNames.Sub,
                        JwtRegisteredClaimNames.Iss,
                        JwtRegisteredClaimNames.Aud,
                        JwtRegisteredClaimNames.Exp,
                        JwtRegisteredClaimNames.Iat,
                        JwtRegisteredClaimNames.Name,
                        JwtRegisteredClaimNames.Email,
                        "scope",
                        "idp"
                    ]
                };
                return Results.Json(discoveryDoc);
            });
            return endpointRouteBuilder;
        }
    }
}