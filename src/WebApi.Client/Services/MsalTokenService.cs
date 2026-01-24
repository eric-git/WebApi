using System.Diagnostics.CodeAnalysis;
using Microsoft.Identity.Client;
using WebApi.Common.Handler;

namespace WebApi.Client.Services;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class MsalTokenService(IConfidentialClientApplication confidentialClientApplication) : ITokenService
{
    public async Task<string> GetTokenAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string[] scopes = ["api.read", "api.write"];
        var acquireTokenForClientParameterBuilder = confidentialClientApplication.AcquireTokenForClient(scopes);
        var result = await acquireTokenForClientParameterBuilder.ExecuteAsync(cancellationToken).ConfigureAwait(false);
        return result.AccessToken;
    }
}