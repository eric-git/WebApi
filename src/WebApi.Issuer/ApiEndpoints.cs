using System.Collections.Immutable;
using System.Globalization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Issuer.DataAccess;
using static WebApi.Common.SecurityExtensions;
using static WebApi.Common.Web.MiddlewareExtensions;

namespace WebApi.Issuer;

internal static class ApiEndpoints
{
    private const string ConnectRouteName = "Connect";
    private const string TokenRouteName = "Token";
    private const string JwksRouteName = "Jwks";
    private const string DocumentRouteName = "Document";

    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapConnect()
        {
            var connect = endpointRouteBuilder.MapGroup("/connect");

            connect.MapGet("/", () => Results.RedirectToRoute(RootRouteName)).WithName(ConnectRouteName);

            connect.MapPost("/token",
                    async (HttpContext context, IConfiguration configuration, LinkGenerator linkGenerator, ISettingsDataRepository settingsDataRepository, CancellationToken cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var formData = await context.Request.ReadFormAsync(cancellationToken).ConfigureAwait(false);
                        var clientId = Guid.Parse(formData["client_id"].ToString());
                        var clientAssertion = formData["client_assertion"].ToString();
                        var scope = formData["scope"].ToString();
                        JsonWebTokenHandler jsonWebTokenHandler = new();
                        var jsonWebToken = jsonWebTokenHandler.ReadJsonWebToken(clientAssertion);
                        var keyId = Guid.Parse(jsonWebToken.Kid);
                        var audience = jsonWebToken.Audiences.FirstOrDefault();
                        var apiId = Guid.Parse(audience!);
                        var publicSigningKey = await settingsDataRepository
                            .GetSigningKeyByClientIdAsync(clientId, apiId, keyId, cancellationToken)
                            .ConfigureAwait(false);
                        if (string.IsNullOrWhiteSpace(publicSigningKey))
                        {
                            return Results.Unauthorized();
                        }

                        var (_, rsaPublicSecurityKey) = CreateRsaSecurityKeyFromPem(publicSigningKey, jsonWebToken.Kid);
                        var issuerBaseUrl = linkGenerator.GetUriByRouteValues(context, ConnectRouteName);
                        var validationResult = await jsonWebTokenHandler.ValidateTokenAsync(
                            jsonWebToken,
                            new TokenValidationParameters
                            {
                                ValidateIssuerSigningKey = true,
                                IssuerSigningKey = rsaPublicSecurityKey,
                                ValidIssuer = issuerBaseUrl,
                                ValidAudience = audience
                            }).ConfigureAwait(false);
                        if (!validationResult.IsValid)
                        {
                            return Results.Unauthorized();
                        }

                        var requestedScopes = scope.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        var allowed = await settingsDataRepository
                            .VerifyClientAccessAsync(clientId, apiId, requestedScopes, cancellationToken)
                            .ConfigureAwait(false);
                        if (!allowed)
                        {
                            return Results.Unauthorized();
                        }

                        var client = await settingsDataRepository
                            .GetClientDetailsById(clientId, cancellationToken)
                            .ConfigureAwait(false);
                        var privateSigningKey = configuration["private-signing-key.pem"];
                        var (_, rsaPrivateSecurityKey) = CreateRsaSecurityKeyFromPem(privateSigningKey!);
                        var tokenLifeTimeInMinutes = int.Parse(
                            configuration["TOKEN_LIFETIME_MINUTES"]!,
                            NumberStyles.Integer,
                            NumberFormatInfo.InvariantInfo);
                        var now = DateTime.UtcNow;
                        SecurityTokenDescriptor descriptor = new()
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
                                { "idp", issuerBaseUrl! }
                            },
                            NotBefore = now,
                            Expires = now.AddMinutes(tokenLifeTimeInMinutes),
                            SigningCredentials = new SigningCredentials(
                                rsaPrivateSecurityKey,
                                SecurityAlgorithms.RsaSha256)
                        };
                        var accessToken = jsonWebTokenHandler.CreateToken(descriptor);
                        return Results.Json(new
                        {
                            token_type = "Bearer",
                            expires_in = tokenLifeTimeInMinutes * 60,
                            access_token = accessToken
                        });
                    })
                .WithName(TokenRouteName);

            connect.MapGet("/.well-known/jwks.json",
                    (IConfiguration configuration, CancellationToken cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var publicSigningKey = configuration["public-signing-key.pem"];
                        var (rsa, rsaSecurityKey) = CreateRsaSecurityKeyFromPem(publicSigningKey!);
                        var parameters = rsa.ExportParameters(false);
                        var jwk = new
                        {
                            kty = JsonWebAlgorithmsKeyTypes.RSA,
                            use = JsonWebKeyUseNames.Sig,
                            kid = rsaSecurityKey.KeyId,
                            e = Base64UrlEncoder.Encode(parameters.Exponent),
                            n = Base64UrlEncoder.Encode(parameters.Modulus)
                        };
                        return Results.Json(new { keys = new[] { jwk } });
                    })
                .WithName(JwksRouteName);

            connect.MapGet("/.well-known/openid-configuration",
                    (LinkGenerator linkGenerator, HttpContext context, CancellationToken cancellationToken) =>
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var signingAlgs = ImmutableArray.Create("RS256");
                        var responseTypes = ImmutableArray.Create("client_credentials");
                        var grantTypes = ImmutableArray.Create("client_credentials");
                        var claimsSupported = ImmutableArray.Create(
                            JwtRegisteredClaimNames.Sub,
                            JwtRegisteredClaimNames.Iss,
                            JwtRegisteredClaimNames.Aud,
                            JwtRegisteredClaimNames.Exp,
                            JwtRegisteredClaimNames.Iat,
                            JwtRegisteredClaimNames.Name,
                            JwtRegisteredClaimNames.Email,
                            "scope",
                            "idp"
                        );
                        var jwksUri = linkGenerator.GetUriByRouteValues(context, JwksRouteName);
                        var tokenEndpoint = linkGenerator.GetUriByRouteValues(context, TokenRouteName);
                        var issuerBaseUrl = linkGenerator.GetUriByRouteValues(context, ConnectRouteName);
                        var discoveryDoc = new
                        {
                            issuer = issuerBaseUrl,
                            token_endpoint = tokenEndpoint,
                            jwks_uri = jwksUri,
                            id_token_signing_alg_values_supported = signingAlgs,
                            response_types_supported = responseTypes,
                            grant_types_supported = grantTypes,
                            claims_supported = claimsSupported
                        };
                        return Results.Json(discoveryDoc);
                    })
                .WithName(DocumentRouteName);

            return endpointRouteBuilder;
        }
    }
}