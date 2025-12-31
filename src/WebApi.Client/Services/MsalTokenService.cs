using Microsoft.Identity.Client;

namespace WebApi.Client.Services;

public class MsalTokenService(IConfidentialClientApplication confidentialClientApplication) : ITokenService
{
    public async Task<string> GetTokenAsync()
    {
        string[] scopes = ["api.read", "api.write"];
        var acquireTokenForClientParameterBuilder = confidentialClientApplication.AcquireTokenForClient(scopes);
        var result = await acquireTokenForClientParameterBuilder.ExecuteAsync();
        return result.AccessToken;
    }
}