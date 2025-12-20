using Microsoft.Identity.Client;

namespace WebApi.Client.Services;

public class MsalHttpClientFactory(IHttpClientFactory factory) : IMsalHttpClientFactory
{
    public const string HttpClientName = "msal";

    public HttpClient GetHttpClient()
    {
        return factory.CreateClient(HttpClientName);
    }
}