using WebApi.Client.Model;

namespace WebApi.Client;

internal interface IServiceClient
{
    Task<string?> CreateGameAsync(Game game, CancellationToken cancellationToken);

    Task UpdateGameAsync(Game game, CancellationToken cancellationToken);

    Task<Game?> GetGameAsync(string id, CancellationToken cancellationToken);

    Task DeleteGameAsync(string id, CancellationToken cancellationToken);

    Task<List<Game>> ListGamesAsync(CancellationToken cancellationToken);
}