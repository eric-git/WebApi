using WebApi.Service.Model;

namespace WebApi.Service.DataAccess;

internal interface IGameDataRepository
{
    Task<List<GameListItem>> GetGamesAsync(CancellationToken cancellationToken);

    Task<Guid> CreateGameAsync(CreateGame game, CancellationToken cancellationToken);

    Task UpdateGameAsync(Guid id, UpdateGame game, CancellationToken cancellationToken);

    Task DeleteGameAsync(Guid id, CancellationToken cancellationToken);

    Task<Game> GetGameByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<List<RelationListItem>> GetRelationsAsync(Guid gameId, CancellationToken cancellationToken);

    Task<Guid> CreateRelationAsync(Guid gameId, CreateRelation relation, CancellationToken cancellationToken);

    Task UpdateRelationAsync(Guid gameId, Guid id, UpdateRelation relation, CancellationToken cancellationToken);

    Task DeleteRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken);

    Task<Relation> GetRelationByIdAsync(Guid gameId, Guid id, CancellationToken cancellationToken);
}