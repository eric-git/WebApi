using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using WebApi.Service.Model;
using static WebApi.Common.Constants;

namespace WebApi.Service.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class JsonFileGameDataRepository : IGameDataRepository
{
    private readonly string _dataFilePath;

    public JsonFileGameDataRepository(IConfiguration configuration, IHostEnvironment hostEnvironment)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        var path = Path.Combine(configuration["DATA_PATH"]!, "db.data");
        if (!Path.IsPathRooted(path))
        {
            path = Path.Combine(hostEnvironment.ContentRootPath, path);
        }

        _dataFilePath = path;
    }

    public async Task<List<GameListItem>> GetGamesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        return data.Select(GameListItem.FromEntity).ToList();
    }

    public async Task<Guid> CreateGameAsync(CreateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var entity = game.ToEntity();
        data.Add(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }

    public async Task UpdateGameAsync(Guid id, UpdateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var entity = data.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        game.UpdateEntity(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteGameAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var entity = data.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        data.Remove(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Game> GetGameByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var entity = data.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        return Game.FromEntity(entity);
    }

    public async Task<List<RelationListItem>> GetRelationsAsync(Guid gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var game = data.SingleOrDefault(x => x.Id == gameId)
                   ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        return game.Relations.Select(RelationListItem.FromEntity).ToList();
    }

    public async Task<Guid> CreateRelationAsync(Guid gameId, CreateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var game = data.SingleOrDefault(x => x.Id == gameId)
                   ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        var entity = relation.ToEntity(gameId);
        game.Relations.Add(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
        return entity.Id;
    }

    public async Task UpdateRelationAsync(Guid gameId, Guid id, UpdateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var game = data.SingleOrDefault(x => x.Id == gameId)
                   ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        var entity = game.Relations.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Relation with ID {id} not found.");
        relation.UpdateEntity(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var game = data.SingleOrDefault(x => x.Id == gameId)
                   ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        var entity = game.Relations.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Relation with ID {id} not found.");
        game.Relations.Remove(entity);
        await SaveAsync(data, cancellationToken).ConfigureAwait(false);
    }

    public async Task<Relation> GetRelationByIdAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var data = await LoadAsync(cancellationToken).ConfigureAwait(false);
        var game = data.SingleOrDefault(x => x.Id == gameId)
                   ?? throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        var entity = game.Relations.SingleOrDefault(x => x.Id == id)
                     ?? throw new KeyNotFoundException($"Relation with ID {id} not found.");
        return Relation.FromEntity(entity);
    }

    private async Task<List<Entity.Game>> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        List<Game>? data;
        var stream = new FileStream(_dataFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        await using (stream)
        {
            data = await JsonSerializer.DeserializeAsync<List<Game>>(stream, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
        }

        var result = (data ?? []).Select(x => x.ToEntityForJson()).ToList();
        return result;
    }

    private async Task SaveAsync(List<Entity.Game> data, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = data.Select(Game.FromEntity).ToList();
        FileStream stream = new(_dataFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
        await using (stream)
        {
            await JsonSerializer.SerializeAsync(stream, result, DataSerializationOptions, cancellationToken).ConfigureAwait(false);
        }
    }
}