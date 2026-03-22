using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebApi.Service.Model;

namespace WebApi.Service.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class PostgresGameDataRepository(AppDbContext appDbContext) : IGameDataRepository
{
    public async Task<List<GameListItem>> GetGamesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await appDbContext.Games
                                       .AsNoTracking()
                                       .ToListAsync(cancellationToken)
                                       .ConfigureAwait(false);
        var model = result.Select(GameListItem.FromEntity).ToList();
        return model;
    }

    public async Task<Guid> CreateGameAsync(CreateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = game.ToEntity();
        var result = await appDbContext.Games.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result.Entity.Id;
    }

    public async Task UpdateGameAsync(Guid id, UpdateGame game, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = await InternalGetGameByIdAsync(id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        game.UpdateEntity(entity);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteGameAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = await InternalGetGameByIdAsync(id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        appDbContext.Games.Remove(entity);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Game> GetGameByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var entity = await InternalGetGameByIdAsync(id, cancellationToken, true).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        var model = Game.FromEntity(entity);
        return model;
    }

    public async Task<List<RelationListItem>> GetRelationsAsync(Guid gameId, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var gameExists = await GameExistsAsync(gameId, cancellationToken).ConfigureAwait(false);
        if (!gameExists)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        var result = await appDbContext.Relations
                                       .AsNoTracking()
                                       .Where(x => x.GameId == gameId)
                                       .ToListAsync(cancellationToken)
                                       .ConfigureAwait(false);
        var model = result.Select(RelationListItem.FromEntity).ToList();
        return model;
    }

    public async Task<Guid> CreateRelationAsync(Guid gameId, CreateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var gameExists = await GameExistsAsync(gameId, cancellationToken).ConfigureAwait(false);
        if (!gameExists)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        var entity = relation.ToEntity(gameId);
        var result = await appDbContext.Relations.AddAsync(entity, cancellationToken).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
        return result.Entity.Id;
    }

    public async Task UpdateRelationAsync(Guid gameId, Guid id, UpdateRelation relation, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var gameExists = await GameExistsAsync(gameId, cancellationToken).ConfigureAwait(false);
        if (!gameExists)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        var entity = await InternalGetRelationByIdAsync(gameId, id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Relation with ID {gameId} not found.");
        relation.UpdateEntity(entity);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task DeleteRelationAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var gameExists = await GameExistsAsync(gameId, cancellationToken).ConfigureAwait(false);
        if (!gameExists)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        var entity = await InternalGetRelationByIdAsync(gameId, id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Relation with ID {gameId} not found.");
        appDbContext.Relations.Remove(entity);
        await appDbContext.SaveChangesAsync(cancellationToken).ConfigureAwait(false);
    }

    public async Task<Relation> GetRelationByIdAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var gameExists = await GameExistsAsync(gameId, cancellationToken).ConfigureAwait(false);
        if (!gameExists)
        {
            throw new KeyNotFoundException($"Game with ID {gameId} not found.");
        }

        var entity = await InternalGetRelationByIdAsync(gameId, id, cancellationToken).ConfigureAwait(false)
                     ?? throw new KeyNotFoundException($"Relation with ID {gameId} not found.");
        var model = Relation.FromEntity(entity);
        return model;
    }

    private async Task<Entity.Game?> InternalGetGameByIdAsync(Guid id, CancellationToken cancellationToken, bool includeRelations = false)
    {
        Entity.Game? entity;
        if (includeRelations)
        {
            entity = await appDbContext.Games
                                       .Include(x => x.Relations)
                                       .SingleOrDefaultAsync(x => x.Id == id, cancellationToken)
                                       .ConfigureAwait(false);
        }
        else
        {
            entity = await appDbContext.Games
                                       .FindAsync([id], cancellationToken)
                                       .ConfigureAwait(false);
        }

        return entity;
    }

    private async Task<bool> GameExistsAsync(Guid id, CancellationToken cancellationToken)
    {
        return await appDbContext.Games
                                 .AnyAsync(x => x.Id == id, cancellationToken)
                                 .ConfigureAwait(false);
    }

    private async Task<Entity.Relation?> InternalGetRelationByIdAsync(Guid gameId, Guid id, CancellationToken cancellationToken)
    {
        var entity = await appDbContext.Relations
                                       .SingleOrDefaultAsync(x => x.GameId == gameId && x.Id == id, cancellationToken)
                                       .ConfigureAwait(false);
        return entity;
    }
}