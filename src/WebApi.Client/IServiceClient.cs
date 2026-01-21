using WebApi.Client.Model;

namespace WebApi.Client;

internal interface IServiceClient
{
    Task<string?> CreateGameAsync(Game game);

    Task UpdateGameAsync(Game game);

    Task<Game?> GetGameAsync(string id);

    Task DeleteGameAsync(string id);

    Task<List<Game>> ListGamesAsync();
}