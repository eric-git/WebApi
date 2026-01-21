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

    public async Task<List<Game>> GetGamesAsync()
    {
#pragma warning disable CA2007
        await using var fileStream = File.OpenRead(_dataFilePath);
#pragma warning restore CA2007
        var games = await JsonSerializer.DeserializeAsync<List<Game>>(fileStream, DataSerializationOptions).ConfigureAwait(false);
        return games ?? [];
    }

    public async Task<string> CreateGameAsync(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        game.Id = Guid.NewGuid().ToString("D");
        foreach (var relation in game.Relations ?? [])
        {
            relation.Id = Guid.NewGuid().ToString("D");
        }

        var games = await GetGamesAsync().ConfigureAwait(false);
        games.Add(game);
#pragma warning disable CA2007
        await using var fileStream = File.Create(_dataFilePath);
#pragma warning restore CA2007
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions).ConfigureAwait(false);
        return game.Id;
    }

    public async Task UpdateGameAsync(Game game)
    {
        var games = await GetGamesAsync().ConfigureAwait(false);
        var index = games.FindIndex(g => GuidsEqual(g.Id, game.Id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{game.Id}' not found.");
        }

        games[index] = game;
#pragma warning disable CA2007
        await using var fileStream = File.Create(_dataFilePath);
#pragma warning restore CA2007
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions).ConfigureAwait(false);
    }

    public async Task DeleteGameAsync(string id)
    {
        var games = await GetGamesAsync().ConfigureAwait(false);
        var index = games.FindIndex(g => GuidsEqual(g.Id, id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{id}' not found.");
        }

        games.RemoveAt(index);
#pragma warning disable CA2007
        await using var fileStream = File.Create(_dataFilePath);
#pragma warning restore CA2007
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions).ConfigureAwait(false);
    }

    public async Task<Game?> GetGameByIdAsync(string id)
    {
        var allGames = await GetGamesAsync().ConfigureAwait(false);
        var game = allGames.Find(x => GuidsEqual(x.Id, id));
        return game;
    }
}