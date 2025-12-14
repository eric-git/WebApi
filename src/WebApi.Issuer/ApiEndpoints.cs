using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.Model;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Issuer;

public static class ApiEndpoints
{
    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapToken(string issuerBaseUrl, int tokenLifeTimeInMinutes)
        {
            endpointRouteBuilder.MapPost("{tenantId}/token", async (HttpRequest httpRequest, string tenantId) =>
            {
                var formData = await httpRequest.ReadFormAsync();
                var clientId = formData["client_id"].ToString();
                var clientAssertion = formData["client_assertion"].ToString();
                var scope = formData["scope"].ToString();
                var settings = JsonSerializer.Deserialize<Settings>(await File.ReadAllTextAsync("./data/db.json"));
                var client = settings!.Clients
                    .SingleOrDefault(c => c.Id == clientId &&
                                          c.Services.Any(s => s.Id == tenantId && s.Scopes.Contains(scope)));
                if (client is null)
                {
                    return Results.Unauthorized();
                }

                var (_, rsaPublicSecurityKey) = await CreateRsaSecurityKeyFromPemFile($"./signing/{client.Id}-public.pem");
                JwtSecurityTokenHandler handler = new();
                try
                {
                    handler.ValidateToken(clientAssertion, new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = rsaPublicSecurityKey,
                        ValidateIssuer = true,
                        ValidIssuer = clientId,
                        ValidateAudience = true,
                        ValidAudience = $"{issuerBaseUrl}/token",
                        ValidateLifetime = true
                    }, out _);
                }
                catch
                {
                    return Results.Unauthorized();
                }

                var (_, rsaPrivateSecurityKey) = await CreateRsaSecurityKeyFromPemFile("./signing/private.pem");
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
                    tenantId,
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
            endpointRouteBuilder.MapGet("/.well-known/jwks.json", async () =>
            {
                var (rsa, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFile("./signing/public.pem");
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
                }, new JsonSerializerOptions { WriteIndented = true });
            });
            return endpointRouteBuilder;
        }
    }
}