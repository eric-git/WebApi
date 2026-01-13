using System.Text.Json;
using WebApi.Service.Model;
using static WebApi.Common.Constants;
using static WebApi.Common.TypeExtensions;

namespace WebApi.Service.DataAccess;

public class JsonFileGameDataRepository(IConfiguration configuration) : IGameDataRepository
{
    private readonly string _dataFilePath = Path.Combine(configuration["DATA_PATH"]!, "db.data");

    public async Task<List<Game>> GetGamesAsync()
    {
        await using var fileStream = File.OpenRead(_dataFilePath);
        var games = await JsonSerializer.DeserializeAsync<List<Game>>(fileStream, DataSerializationOptions);
        return games ?? [];
    }

    public async Task<string> CreateGameAsync(Game game)
    {
        game.Id = Guid.NewGuid().ToString("D");
        foreach (var relation in game.Relations ?? [])
        {
            relation.Id = Guid.NewGuid().ToString("D");
        }

        var games = await GetGamesAsync();
        games.Add(game);
        await using var fileStream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions);
        return game.Id;
    }

    public async Task UpdateGameAsync(Game game)
    {
        var games = await GetGamesAsync();
        var index = games.FindIndex(g => GuidsEqual(g.Id, game.Id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{game.Id}' not found.");
        }

        games[index] = game;
        await using var fileStream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions);
    }

    public async Task DeleteGameAsync(string id)
    {
        var games = await GetGamesAsync();
        var index = games.FindIndex(g => GuidsEqual(g.Id, id));
        if (index is -1)
        {
            throw new KeyNotFoundException($"Game with ID '{id}' not found.");
        }

        games.RemoveAt(index);
        await using var fileStream = File.Create(_dataFilePath);
        await JsonSerializer.SerializeAsync(fileStream, games, DataSerializationOptions);
    }

    public Task<Game?> GetGameByIdAsync(string id)
    {
        return GetGamesAsync().ContinueWith(t => t.Result.Find(g => GuidsEqual(g.Id, id)));
    }
}