using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using WebApi.Client.Model;

namespace WebApi.Client;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class ServiceClient(IHttpClientFactory httpClientFactory) : IServiceClient
{
    private const string GamesPath = "games";
    public const string HttpClientName = "api-client";

    public async Task<string?> CreateGameAsync(Game game)
    {
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, GamesPath);
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<string>().ConfigureAwait(false);
        return data;
    }

    public async Task UpdateGameAsync(Game game)
    {
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Patch, GamesPath);
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
    }

    public async Task<Game?> GetGameAsync(string id)
    {
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        var game = await httpResponseMessage.Content.ReadFromJsonAsync<Game>().ConfigureAwait(false);
        return game;
    }

    public async Task DeleteGameAsync(string id)
    {
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
    }

    public async Task<List<Game>> ListGamesAsync()
    {
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, GamesPath);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage).ConfigureAwait(false);
        var list = await httpResponseMessage.Content.ReadFromJsonAsync<List<Game>>().ConfigureAwait(false);
        return list ?? [];
    }
}