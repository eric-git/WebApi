using System.Net.Http.Headers;
using WebApi.Client.Services;

namespace WebApi.Client.Handlers;

public class AccessTokenHandler(ITokenService tokenService) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage httpRequestMessage, CancellationToken cancellationToken)
    {
        var token = await tokenService.GetTokenAsync();
        httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(httpRequestMessage, cancellationToken);
    }
}