using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebApi.Service.Model;
using static WebApi.Common.Constants;
using static WebApi.Common.TypeExtensions;

namespace WebApi.Service.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class JsonFileGameDataRepository : IGameDataRepository
{
    private readonly string _dataFilePath;

    public JsonFileGameDataRepository(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var dataFilePath = Path.Combine(configuration["DATA_PATH"]!, "db.data");
        if (!Path.IsPathRooted(dataFilePath))
        {
            dataFilePath = Path.Combine(AppContext.BaseDirectory, dataFilePath);
        }

        _dataFilePath = dataFilePath;
    }

    public async Task<List<Game>> GetGamesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var fileStream = File.OpenRead(_dataFilePath);
        await using (fileStream)
        {
            var games = await JsonSerializer.DeserializeAsync<List<Game>>(fileStream, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
            return games ?? [];
        }
    }

    public async Task<string> CreateGameAsync(Game game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(game);
        game.Id = Guid.NewGuid().ToString("D");
        foreach (var relation in game.Relations ?? [])
        {
            relation.Id = Guid.NewGuid().ToString("D");
        }

        var games = await GetGamesAsync(cancellationToken).ConfigureAwait(false);
        games.Add(game);
        var fileStream = File.Create(_dataFilePath);
        await using (fileStream)
        {
            await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
            return game.Id;
        }
    }

    public async Task UpdateGameAsync(Game game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var games = await GetGamesAsync(cancellationToken).ConfigureAwait(false);
        var index = games.FindIndex(g => GuidsEqual(g.Id, game.Id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{game.Id}' not found.");
        }

        games[index] = game;
        var fileStream = File.Create(_dataFilePath);
        await using (fileStream)
        {
            await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task DeleteGameAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var games = await GetGamesAsync(cancellationToken).ConfigureAwait(false);
        var index = games.FindIndex(g => GuidsEqual(g.Id, id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{id}' not found.");
        }

        games.RemoveAt(index);
        var fileStream = File.Create(_dataFilePath);
        await using (fileStream)
        {
            await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
        }
    }

    public async Task<Game?> GetGameByIdAsync(string id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var allGames = await GetGamesAsync(cancellationToken).ConfigureAwait(false);
        var game = allGames.Find(x => GuidsEqual(x.Id, id));
        return game;
    }
}