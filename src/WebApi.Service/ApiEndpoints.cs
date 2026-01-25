using Microsoft.AspNetCore.Mvc;
using WebApi.Service.DataAccess;
using WebApi.Service.Model;

namespace WebApi.Service;

internal static class ApiEndpoints
{
    private const string GetGameRouteName = "GetGame";
    private const string CreateGameRouteName = "CreateGame";
    private const string UpdateGameRouteName = "UpdateGame";
    private const string DeleteGameRouteName = "DeleteGame";
    private const string ListGamesRouteName = "ListGames";

    private const string GetRelationRouteName = "GetRelation";
    private const string CreateRelationRouteName = "CreateRelation";
    private const string UpdateRelationRouteName = "UpdateRelation";
    private const string DeleteRelationRouteName = "DeleteRelation";
    private const string ListRelationsRouteName = "ListRelations";

    public const string ReadPolicyName = "APIRead";
    public const string WritePolicyName = "APIWrite";

    public static IEndpointRouteBuilder MapGame(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var games = endpointRouteBuilder.MapGroup("/games")
            .WithTags("Games")
            .WithMetadata(
                new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError),
                new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden)
            );

        games.MapGet("/", async (IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                return await gameDataRepository.GetGamesAsync(cancellationToken).ConfigureAwait(false);
            })
            .WithName(ListGamesRouteName)
            .WithDisplayName("List all games")
            .WithSummary("List all games")
            .WithDescription("Returns all games stored in the system.")
            .Produces<List<Game>>()
            .RequireAuthorization(ReadPolicyName);

        games.MapGet("/{id:guid}", async (Guid id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return Results.Ok(await gameDataRepository.GetGameByIdAsync(id, cancellationToken).ConfigureAwait(false));
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(GetGameRouteName)
            .WithDisplayName("Get a single game")
            .WithSummary("Get a single game")
            .WithDescription("Returns a game by its unique identifier.")
            .Produces<Game>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(ReadPolicyName);

        games.MapPost("/", async (CreateGame game, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var id = await gameDataRepository.CreateGameAsync(game, cancellationToken).ConfigureAwait(false);
                return Results.CreatedAtRoute(GetGameRouteName, new { id }, id);
            })
            .WithName(CreateGameRouteName)
            .WithDisplayName("Create a new game")
            .WithSummary("Create a new game")
            .WithDescription("Creates a new game and returns its identifier.")
            .Accepts<CreateGame>("application/json")
            .Produces<string>(StatusCodes.Status201Created)
            .RequireAuthorization(WritePolicyName);

        games.MapPatch("/{id:guid}", async (Guid id, UpdateGame game, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.UpdateGameAsync(id, game, cancellationToken).ConfigureAwait(false);
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(UpdateGameRouteName)
            .WithDisplayName("Update a game")
            .WithSummary("Update a game")
            .WithDescription("Updates the specified game with new values.")
            .Accepts<UpdateGame>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(WritePolicyName);

        games.MapDelete("/{id:guid}", async (Guid id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.DeleteGameAsync(id, cancellationToken).ConfigureAwait(false);
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(DeleteGameRouteName)
            .WithDisplayName("Delete a game")
            .WithSummary("Delete a game")
            .WithDescription("Deletes the specified game.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(WritePolicyName);

        var relations = games.MapGroup("/{gameId:guid}/relations")
            .WithTags("Relations")
            .WithMetadata(
                new ProducesResponseTypeAttribute(StatusCodes.Status500InternalServerError),
                new ProducesResponseTypeAttribute(StatusCodes.Status401Unauthorized),
                new ProducesResponseTypeAttribute(StatusCodes.Status403Forbidden)
            );

        relations.MapGet("/", async (Guid gameId, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return Results.Ok(await gameDataRepository.GetRelationsAsync(gameId, cancellationToken).ConfigureAwait(false));
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(ListRelationsRouteName)
            .WithDisplayName("List relations for a game")
            .WithSummary("List relations for a game")
            .WithDescription("Returns all relations associated with the specified game.")
            .Produces<List<Relation>>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(ReadPolicyName);

        relations.MapGet("/{id:guid}", async (Guid gameId, Guid id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    return Results.Ok(await gameDataRepository.GetRelationByIdAsync(gameId, id, cancellationToken).ConfigureAwait(false));
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(GetRelationRouteName)
            .WithDisplayName("Get a relation")
            .WithSummary("Get a relation")
            .WithDescription("Returns a relation belonging to a specific game.")
            .Produces<Relation>()
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(ReadPolicyName);

        relations.MapPost("/", async (Guid gameId, CreateRelation relation, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    var id = await gameDataRepository.CreateRelationAsync(gameId, relation, cancellationToken).ConfigureAwait(false);
                    return Results.CreatedAtRoute(GetRelationRouteName, new { gameId, id }, id);
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(CreateRelationRouteName)
            .WithDisplayName("Create a relation")
            .WithSummary("Create a relation")
            .WithDescription("Creates a new relation under the specified game.")
            .Accepts<CreateRelation>("application/json")
            .Produces<string>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(WritePolicyName);

        relations.MapPatch("/{id:guid}", async (Guid gameId, Guid id, UpdateRelation relation, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.UpdateRelationAsync(gameId, id, relation, cancellationToken).ConfigureAwait(false);
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(UpdateRelationRouteName)
            .WithDisplayName("Update a relation")
            .WithSummary("Update a relation")
            .WithDescription("Updates the specified relation under the given game.")
            .Accepts<UpdateRelation>("application/json")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(WritePolicyName);

        relations.MapDelete("/{id:guid}", async (Guid gameId, Guid id, IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                try
                {
                    await gameDataRepository.DeleteRelationAsync(gameId, id, cancellationToken).ConfigureAwait(false);
                    return Results.NoContent();
                }
                catch (KeyNotFoundException ex)
                {
                    return Results.NotFound(ex.Message);
                }
            })
            .WithName(DeleteRelationRouteName)
            .WithDisplayName("Delete a relation")
            .WithSummary("Delete a relation")
            .WithDescription("Deletes the specified relation from the game.")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization(WritePolicyName);

        return endpointRouteBuilder;
    }
}