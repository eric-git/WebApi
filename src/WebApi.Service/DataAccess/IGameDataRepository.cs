using WebApi.Service.Model;

namespace WebApi.Service.DataAccess;

internal interface IGameDataRepository
{
    Task<List<Game>> GetGamesAsync(CancellationToken cancellationToken);

    Task<string> CreateGameAsync(Game game, CancellationToken cancellationToken);

    Task UpdateGameAsync(Game game, CancellationToken cancellationToken);

    Task DeleteGameAsync(string id, CancellationToken cancellationToken);

    Task<Game?> GetGameByIdAsync(string id, CancellationToken cancellationToken);
}