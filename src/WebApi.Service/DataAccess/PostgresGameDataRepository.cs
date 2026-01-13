using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Game = WebApi.Service.DataAccess.Entity.Game;

namespace WebApi.Service.DataAccess;

public class PostgresGameDataRepository(AppDbContext appDbContext, IMapper mapper) : IGameDataRepository
{
    public async Task<List<Model.Game>> GetGamesAsync()
    {
        var result = await appDbContext.Games
            .AsNoTracking()
            .Include(x => x.Relations)
            .ToListAsync();
        var model = mapper.Map<List<Game>, List<Model.Game>>(result);
        return model;
    }

    public async Task<string> CreateGameAsync(Model.Game game)
    {
        var entity = mapper.Map<Model.Game, Game>(game);
        var result = await appDbContext.Games.AddAsync(entity);
        await appDbContext.SaveChangesAsync();
        return result.Entity.Id.ToString();
    }

    public async Task UpdateGameAsync(Model.Game game)
    {
        var entityId = Guid.Parse(game.Id!);
        var entity = await appDbContext.Games
            .Include(x => x.Relations)
            .SingleOrDefaultAsync(x => x.Id == entityId) ?? throw new KeyNotFoundException($"Game with ID {game.Id} not found.");
        mapper.Map(game, entity);
        await appDbContext.SaveChangesAsync();
    }

    public async Task DeleteGameAsync(string id)
    {
        var entity = await appDbContext.Games.FindAsync(Guid.Parse(id)) ?? throw new KeyNotFoundException($"Game with ID {id} not found.");
        appDbContext.Games.Remove(entity);
        await appDbContext.SaveChangesAsync();
    }

    public async Task<Model.Game?> GetGameByIdAsync(string id)
    {
        var entityId = Guid.Parse(id);
        var entity = await appDbContext.Games
            .Include(x => x.Relations)
            .SingleOrDefaultAsync(x => x.Id == entityId);
        if (entity is null)
        {
            return null;
        }

        var model = mapper.Map<Game, Model.Game>(entity);
        return model;
    }
}