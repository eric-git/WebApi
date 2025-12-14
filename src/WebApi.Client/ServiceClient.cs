using System.Net.Http.Json;
using WebApi.Client.Model;

namespace WebApi.Client;

public class ServiceClient(IHttpClientFactory httpClientFactory) : IServiceClient
{
    private const string GamesPath = "games";
    public const string HttpClientName = "api-client";

    public async Task<string?> CreateGameAsync(Game game)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Post, GamesPath)
        {
            Content = JsonContent.Create(game)
        };
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        var data = await httpResponseMessage.Content.ReadFromJsonAsync<string>();
        return data;
    }

    public async Task UpdateGameAsync(Game game)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Patch, GamesPath)
        {
            Content = JsonContent.Create(game)
        };
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage);
    }

    public async Task<Game?> GetGameAsync(string id)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        var game = await httpResponseMessage.Content.ReadFromJsonAsync<Game>();
        return game;
    }

    public async Task DeleteGameAsync(string id)
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Delete, $"{GamesPath}/{id}");
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        await httpClient.SendAsync(httpRequestMessage);
    }

    public async Task<List<Game>> ListGamesAsync()
    {
        HttpRequestMessage httpRequestMessage = new(HttpMethod.Get, GamesPath);
        using var httpClient = httpClientFactory.CreateClient(HttpClientName);
        var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);
        var list = await httpResponseMessage.Content.ReadFromJsonAsync<List<Game>>();
        return list ?? [];
    }
}