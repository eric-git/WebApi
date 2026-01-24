using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using WebApi.Client.Model;

namespace WebApi.Client;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class ServiceClient(IHttpClientFactory httpClientFactory) : IServiceClient
{
    private const string GamesPath = "games";
    public const string HttpClientName = "api-client";

    public async Task<string?> CreateGameAsync(Game game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, GamesPath);
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<string>(cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async Task UpdateGameAsync(Game game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Patch, GamesPath);
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Game?> GetGameAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var game = await httpResponseMessage.Content.ReadFromJsonAsync<Game>(cancellationToken).ConfigureAwait(false);
        return game;
    }

    public async Task DeleteGameAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<Game>> ListGamesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, GamesPath);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var list = await httpResponseMessage.Content.ReadFromJsonAsync<List<Game>>(cancellationToken).ConfigureAwait(false);
        return list ?? [];
    }
}