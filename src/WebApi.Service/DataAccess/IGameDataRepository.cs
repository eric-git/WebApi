using WebApi.Service.Model;

namespace WebApi.Service.DataAccess;

internal interface IGameDataRepository
{
    Task<List<Game>> GetGamesAsync();

    Task<string> CreateGameAsync(Game game);

    Task UpdateGameAsync(Game game);

    Task DeleteGameAsync(string id);

    Task<Game?> GetGameByIdAsync(string id);
}