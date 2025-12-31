using System.Text.Json;
using WebApi.Service.Model;
using static WebApi.Common.TypeExtensions;
using static WebApi.Common.Constants;

namespace WebApi.Service;

public static class ApiEndpoints
{
    private const string GamesPath = "games";
    private static readonly string DataFilePath = Path.Combine(DataStoreRootPath, "db.json");

    private static async Task<GameData> GetGameDataAsync()
    {
        await using var fileStream = File.OpenRead(DataFilePath);
        var gameData = await JsonSerializer.DeserializeAsync<GameData>(fileStream, DataSerializationOptions);
        return gameData ?? new GameData();
    }

    private static async Task SaveGameDataAsync(GameData gameData)
    {
        await using var fileStream = File.Create(DataFilePath);
        await JsonSerializer.SerializeAsync(fileStream, gameData, DataSerializationOptions);
    }

    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapGame()
        {
            endpointRouteBuilder.MapGet(GamesPath, async () =>
            {
                var gameData = await GetGameDataAsync();
                return Results.Json(gameData.Games);
            }).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapGet($"{GamesPath}/{{id}}", async (string id) =>
                {
                    var gameData = await GetGameDataAsync();
                    var game = gameData.Games!.SingleOrDefault(x => GuidsEqual(x.Id, id));
                    return game is null ? Results.NotFound() : Results.Json(game);
                }
            ).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapPost(GamesPath, async (Game game, HttpContext httpContext) =>
            {
                var gameData = await GetGameDataAsync();
                game.Id = Guid.NewGuid().ToString("D");
                foreach (var relation in game.Relations ?? [])
                {
                    relation.Id = Guid.NewGuid().ToString("D");
                }

                gameData.Games!.Add(game);
                await SaveGameDataAsync(gameData);
                return Results.Created($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{GamesPath}/{game.Id}", game.Id);
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapPatch(GamesPath, async (Game game) =>
            {
                var gameData = await GetGameDataAsync();
                var index = gameData.Games!.FindIndex(x => GuidsEqual(x.Id, game.Id));
                if (index is -1)
                {
                    return Results.NotFound();
                }

                gameData.Games[index] = game;
                await SaveGameDataAsync(gameData);
                return Results.Ok();
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapDelete($"{GamesPath}/{{id}}", async (string id) =>
            {
                var gameData = await GetGameDataAsync();
                var index = gameData.Games!.FindIndex(x => GuidsEqual(x.Id, id));
                if (index is -1)
                {
                    return Results.NotFound();
                }

                gameData.Games.RemoveAt(index);
                await SaveGameDataAsync(gameData);
                return Results.Ok();
            }).RequireAuthorization("ApiWrite");
            return endpointRouteBuilder;
        }
    }
}