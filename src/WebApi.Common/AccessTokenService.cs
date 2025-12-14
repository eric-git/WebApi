using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using static WebApi.Common.SecurityExtensions;

namespace WebApi.Common;

public class AccessTokenService(string apiId, string clientId, IHttpClientFactory httpClientFactory, IMemoryCache memoryCache) : ITokenService
{
    public const string HttpClientName = "access-token-client";

    private static readonly JsonSerializerOptions TokenSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        PropertyNameCaseInsensitive = true
    };


    public async Task<string> GetTokenAsync()
    {
        var key = $"{nameof(AccessTokenService)}-{apiId}";
        if (memoryCache.TryGetValue<TokenResponse>(key, out var accessToken) && accessToken is not null)
        {
            return accessToken.AccessToken;
        }

        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var (_, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFile("./signing/private.pem");
        SigningCredentials signingCredentials = new(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
        var now = DateTime.UtcNow;
        JwtSecurityToken jwtSecurityToken = new(
            clientId,
            $"{httpClient.BaseAddress!.ToString().TrimEnd('/')}/token",
            [
                new Claim(JwtRegisteredClaimNames.Sub, clientId),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
            ],
            now,
            now.AddMinutes(5),
            signingCredentials
        );
        var clientAssertion = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        Dictionary<string, string> formData = new()
        {
            { "grant_type", "client_credentials" },
            { "client_id", clientId },
            { "scope", "api.read" },
            { "client_assertion_type", "urn:ietf:params:oauth:client-assertion-type:jwt-bearer" },
            { "client_assertion", clientAssertion }
        };
        HttpRequestMessage tokenRequestMessage = new(HttpMethod.Post, $"{httpClient.BaseAddress!.ToString().TrimEnd('/')}/{apiId}/token")
        {
            Content = new FormUrlEncodedContent(formData)
        };
        var tokenResponseMessage = await httpClient.SendAsync(tokenRequestMessage);
        accessToken = await tokenResponseMessage.Content.ReadFromJsonAsync<TokenResponse>(TokenSerializerOptions);
        accessToken!.ExpiryUtc = DateTime.UtcNow.AddSeconds(accessToken.ExpiresIn - 60);
        accessToken.ServiceId = apiId;
        memoryCache.Set(
            key,
            accessToken,
            new MemoryCacheEntryOptions
            {
                AbsoluteExpiration = accessToken.ExpiryUtc
            });
        return accessToken.AccessToken;
    }
}