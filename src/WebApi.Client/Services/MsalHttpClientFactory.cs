using System.Diagnostics.CodeAnalysis;
using Microsoft.Identity.Client;

namespace WebApi.Client.Services;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class MsalHttpClientFactory(IHttpClientFactory factory) : IMsalHttpClientFactory
{
    public const string HttpClientName = "msal";

    public HttpClient GetHttpClient()
    {
        return factory.CreateClient(HttpClientName);
    }
}