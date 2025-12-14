using WebApi.Client.Model;

namespace WebApi.Client;

public interface IServiceClient
{
    Task<string?> CreateGameAsync(Game game);

    Task UpdateGameAsync(Game game);

    Task<Game?> GetGameAsync(string id);

    Task DeleteGameAsync(string id);

    Task<List<Game>> ListGamesAsync();
}