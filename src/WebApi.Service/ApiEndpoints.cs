using WebApi.Service.DataAccess;
using WebApi.Service.Model;

namespace WebApi.Service;

internal static class ApiEndpoints
{
    private const string GamesPath = "games";

    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapGame()
        {
            endpointRouteBuilder.MapGet(GamesPath, async (IGameDataRepository gameDataRepository) =>
            {
                var gameData = await gameDataRepository.GetGamesAsync().ConfigureAwait(false);
                return Results.Json(gameData);
            }).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapGet($"{GamesPath}/{{id}}", async (string id, IGameDataRepository gameDataRepository) =>
                {
                    var game = await gameDataRepository.GetGameByIdAsync(id).ConfigureAwait(false);
                    return game is null ? Results.NotFound() : Results.Json(game);
                }
            ).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapPost(GamesPath, async (Game game, HttpContext httpContext, IGameDataRepository gameDataRepository) =>
            {
                var id = await gameDataRepository.CreateGameAsync(game).ConfigureAwait(false);
                return Results.Created($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{GamesPath}/{id}", id);
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapPatch(GamesPath, async (Game game, IGameDataRepository gameDataRepository) =>
            {
                try
                {
                    await gameDataRepository.UpdateGameAsync(game).ConfigureAwait(false);
                }
                catch (KeyNotFoundException keyNotFoundException)
                {
                    return Results.NotFound(keyNotFoundException.Message);
                }

                return Results.Ok();
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapDelete($"{GamesPath}/{{id}}", async (string id, IGameDataRepository gameDataRepository) =>
            {
                try
                {
                    await gameDataRepository.DeleteGameAsync(id).ConfigureAwait(false);
                }
                catch (KeyNotFoundException keyNotFoundException)
                {
                    return Results.NotFound(keyNotFoundException.Message);
                }

                return Results.Ok();
            }).RequireAuthorization("ApiWrite");
            return endpointRouteBuilder;
        }
    }
}