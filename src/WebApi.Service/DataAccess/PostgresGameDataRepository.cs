using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using WebApi.Service.Model;

namespace WebApi.Service.DataAccess;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class PostgresGameDataRepository(AppDbContext appDbContext) : IGameDataRepository
{
    public async Task<List<Game>> GetGamesAsync()
    {
        var result = await appDbContext.Games
            .AsNoTracking()
            .Include(x => x.Relations)
            .ToListAsync()
            .ConfigureAwait(false);
        var model = result.Select(Game.FromEntity).ToList();
        return model;
    }

    public async Task<string> CreateGameAsync(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var entity = game.ToEntity();
        var result = await appDbContext.Games.AddAsync(entity).ConfigureAwait(false);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);
        return result.Entity.Id.ToString();
    }

    public async Task UpdateGameAsync(Game game)
    {
        ArgumentNullException.ThrowIfNull(game);
        var entityId = Guid.Parse(game.Id!);
        var entity = await appDbContext.Games
            .Include(x => x.Relations)
            .SingleOrDefaultAsync(x => x.Id == entityId)
            .ConfigureAwait(false) ?? throw new KeyNotFoundException($"Game with ID {game.Id} not found.");
        game.ToEntity(entity);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task DeleteGameAsync(string id)
    {
        var entity = await appDbContext.Games
            .FindAsync(Guid.Parse(id))
            .ConfigureAwait(false) ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        appDbContext.Games.Remove(entity);
        await appDbContext.SaveChangesAsync().ConfigureAwait(false);
    }

    public async Task<Game?> GetGameByIdAsync(string id)
    {
        var entityId = Guid.Parse(id);
        var entity = await appDbContext.Games
            .Include(x => x.Relations)
            .SingleOrDefaultAsync(x => x.Id == entityId)
            .ConfigureAwait(false);
        if (entity is null)
        {
            return null;
        }

        var model = Game.FromEntity(entity);
        return model;
    }
}