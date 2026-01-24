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
            endpointRouteBuilder.MapGet(GamesPath, async (IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var gameData = await gameDataRepository.GetGamesAsync(cancellationToken).ConfigureAwait(false);
                return Results.Json(gameData);
            }).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapGet($"{GamesPath}/{{id}}", async (string id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var game = await gameDataRepository.GetGameByIdAsync(id, cancellationToken).ConfigureAwait(false);
                    return game is null ? Results.NotFound() : Results.Json(game);
                }
            ).RequireAuthorization("ApiRead");

            endpointRouteBuilder.MapPost(GamesPath, async (Game game, HttpContext httpContext, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var id = await gameDataRepository.CreateGameAsync(game, cancellationToken).ConfigureAwait(false);
                return Results.Created($"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{GamesPath}/{id}", id);
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapPatch(GamesPath, async (Game game, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.UpdateGameAsync(game, cancellationToken).ConfigureAwait(false);
                }
                catch (KeyNotFoundException keyNotFoundException)
                {
                    return Results.NotFound(keyNotFoundException.Message);
                }

                return Results.Ok();
            }).RequireAuthorization("ApiWrite");

            endpointRouteBuilder.MapDelete($"{GamesPath}/{{id}}", async (string id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.DeleteGameAsync(id, cancellationToken).ConfigureAwait(false);
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