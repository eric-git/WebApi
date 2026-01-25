using WebApi.Client.Model;

namespace WebApi.Client;

internal interface IServiceClient
{
    Task<Guid> CreateGameAsync(CreateGame game, CancellationToken cancellationToken);

    Task UpdateGameAsync(Guid id, UpdateGame game, CancellationToken cancellationToken);

    Task<Game?> GetGameAsync(Guid id, CancellationToken cancellationToken);

    Task DeleteGameAsync(Guid id, CancellationToken cancellationToken);

    Task<List<GameListItem>?> ListGamesAsync(CancellationToken cancellationToken);

    Task<Guid> CreateRelationAsync(Guid gameId, CreateRelation relation, CancellationToken cancellationToken);

    Task UpdateRelationAsync(Guid gameId, Guid id, UpdateRelation relation, CancellationToken cancellationToken);

    Task<Relation?> GetRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken);

    Task DeleteRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken);

    Task<List<RelationListItem>?> ListRelationsAsync(Guid gameId, CancellationToken cancellationToken);
}