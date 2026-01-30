using System.Diagnostics.CodeAnalysis;
using System.Net.Http.Json;
using WebApi.Client.Model;

namespace WebApi.Client.Client;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class GameServiceClient(IHttpClientFactory httpClientFactory) : IGameServiceClient
{
    private const string GamesPath = "games";
    public const string HttpClientName = "api-client";

    public async Task<Guid> CreateGameAsync(CreateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, GamesPath);
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<Guid>(cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async Task UpdateGameAsync(Guid id, UpdateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Patch, $"{GamesPath}/{id}");
        httpRequestMessage.Content = JsonContent.Create(game);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var game = await httpResponseMessage.Content.ReadFromJsonAsync<Game>(cancellationToken).ConfigureAwait(false);
        return game;
    }

    public async Task DeleteGameAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<GameListItem>?> ListGamesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, GamesPath);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var list = await httpResponseMessage.Content.ReadFromJsonAsync<List<GameListItem>>(cancellationToken).ConfigureAwait(false);
        return list;
    }

    public async Task<Guid> CreateRelationAsync(Guid gameId, CreateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, $"{GamesPath}/{gameId}/relations");
        httpRequestMessage.Content = JsonContent.Create(relation);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<Guid>(cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async Task UpdateRelationAsync(Guid gameId, Guid id, UpdateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Patch, $"{GamesPath}/{gameId}/relations/{id}");
        httpRequestMessage.Content = JsonContent.Create(relation);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Relation?> GetRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{gameId}/relations/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<Relation>(cancellationToken).ConfigureAwait(false);
        return data;
    }

    public async Task DeleteRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"{GamesPath}/{gameId}/relations/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
    }

    public async Task<List<RelationListItem>?> ListRelationsAsync(Guid gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        using HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{gameId}/relations");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        using var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);
        var list = await httpResponseMessage.Content.ReadFromJsonAsync<List<RelationListItem>>(cancellationToken).ConfigureAwait(false);
        return list;
    }
}