using System.ComponentModel;
using System.Net.Mime;
using WebApi.Common.Web.Documentation;
using WebApi.Common.Web.Validation;
using WebApi.Service.DataAccess;
using WebApi.Service.Model;

namespace WebApi.Service;

internal static class ApiEndpoints
{
    private const string GamesPrefix = "games";
    private const string GetGameRouteName = "GetGame";
    private const string CreateGameRouteName = "CreateGame";
    private const string UpdateGameRouteName = "UpdateGame";
    private const string DeleteGameRouteName = "DeleteGame";
    private const string ListGamesRouteName = "ListGames";

    private const string RelationsPrefix = "{gameId:guid}/relations";
    private const string GetRelationRouteName = "GetRelation";
    private const string CreateRelationRouteName = "CreateRelation";
    private const string UpdateRelationRouteName = "UpdateRelation";
    private const string DeleteRelationRouteName = "DeleteRelation";
    private const string ListRelationsRouteName = "ListRelations";

    public const string ReadPolicyName = "APIRead";
    public const string WritePolicyName = "APIWrite";

    public static IEndpointRouteBuilder MapGame(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var games = endpointRouteBuilder.MapGroup($"/{GamesPrefix}")
                                        .AddEndpointFilter<ValidationFilter>()
                                        .WithTags("Games")
                                        .WithSummary("Game operations")
                                        .WithDescription("Endpoints for retrieving, creating, and managing games.")
                                        .ProducesProblem(StatusCodes.Status500InternalServerError)
                                        .ProducesProblem(StatusCodes.Status401Unauthorized)
                                        .ProducesProblem(StatusCodes.Status403Forbidden)
                                        .WithMetadata(new TagDescription("Games", "Endpoints for retrieving, creating, and managing games."));

        games.MapGet("/", async (IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 return await gameDataRepository.GetGamesAsync(cancellationToken).ConfigureAwait(false);
             })
             .WithName(ListGamesRouteName)
             .WithDisplayName("List all games")
             .WithSummary("List all games")
             .WithDescription("Returns all games stored in the system.")
             .Produces<List<GameListItem>>()
             .RequireAuthorization(ReadPolicyName);

        games.MapGet("/{id:guid}", async (
                 [Description("The ID of the game")] Guid id,
                 IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 try
                 {
                     return Results.Ok(await gameDataRepository.GetGameByIdAsync(id, cancellationToken).ConfigureAwait(false));
                 }
                 catch (KeyNotFoundException keyNotFoundException)
                 {
                     return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                 }
             })
             .WithName(GetGameRouteName)
             .WithDisplayName("Get a single game")
             .WithSummary("Get a single game")
             .WithDescription("Returns a game by its unique identifier.")
             .Produces<Game>()
             .ProducesProblem(StatusCodes.Status404NotFound)
             .RequireAuthorization(ReadPolicyName);

        games.MapPost("/", async (
                 [Description("The data of the game to be created")]
                 CreateGame game,
                 IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 var id = await gameDataRepository.CreateGameAsync(game, cancellationToken).ConfigureAwait(false);
                 return Results.CreatedAtRoute(GetGameRouteName, new { id }, id);
             })
             .WithName(CreateGameRouteName)
             .WithDisplayName("Create a new game")
             .WithSummary("Create a new game")
             .WithDescription("Creates a new game and returns its identifier.")
             .Accepts<CreateGame>(MediaTypeNames.Application.Json)
             .Produces<Guid>(StatusCodes.Status201Created)
             .WithMetadata(new CreatedLocation("The URL of the newly created game."))
             .ProducesValidationProblem()
             .RequireAuthorization(WritePolicyName);

        games.MapPatch("/{id:guid}", async (
                 [Description("The ID of the game to be updated")]
                 Guid id,
                 [Description("The data of the game to be updated")]
                 UpdateGame game,
                 IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 try
                 {
                     await gameDataRepository.UpdateGameAsync(id, game, cancellationToken).ConfigureAwait(false);
                     return Results.NoContent();
                 }
                 catch (KeyNotFoundException keyNotFoundException)
                 {
                     return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                 }
             })
             .WithName(UpdateGameRouteName)
             .WithDisplayName("Update a game")
             .WithSummary("Update a game")
             .WithDescription("Updates the specified game with new values.")
             .Accepts<UpdateGame>(MediaTypeNames.Application.Json)
             .Produces(StatusCodes.Status204NoContent)
             .ProducesProblem(StatusCodes.Status404NotFound)
             .ProducesValidationProblem()
             .RequireAuthorization(WritePolicyName);

        games.MapDelete("/{id:guid}", async (
                 [Description("The ID of the game to be deleted")]
                 Guid id,
                 IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
             {
                 cancellationToken.ThrowIfCancellationRequested();
                 try
                 {
                     await gameDataRepository.DeleteGameAsync(id, cancellationToken).ConfigureAwait(false);
                     return Results.NoContent();
                 }
                 catch (KeyNotFoundException keyNotFoundException)
                 {
                     return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                 }
             })
             .WithName(DeleteGameRouteName)
             .WithDisplayName("Delete a game")
             .WithSummary("Delete a game")
             .WithDescription("Deletes the specified game.")
             .Produces(StatusCodes.Status204NoContent)
             .ProducesProblem(StatusCodes.Status404NotFound)
             .RequireAuthorization(WritePolicyName);

        var relations = games.MapGroup($"/{RelationsPrefix}")
                             .WithTags("Relations")
                             .WithSummary("Game relation operations")
                             .WithDescription("Endpoints for retrieving, creating, and managing game relations.")
                             .ProducesProblem(StatusCodes.Status500InternalServerError)
                             .ProducesProblem(StatusCodes.Status401Unauthorized)
                             .ProducesProblem(StatusCodes.Status403Forbidden)
                             .ProducesProblem(StatusCodes.Status404NotFound)
                             .WithMetadata(new TagDescription("Relations", "Endpoints for retrieving, creating, and managing game relations."));

        relations.MapGet("/", async (
                     [Description("The ID of the game")] Guid gameId,
                     IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     try
                     {
                         return Results.Ok(await gameDataRepository.GetRelationsAsync(gameId, cancellationToken).ConfigureAwait(false));
                     }
                     catch (KeyNotFoundException keyNotFoundException)
                     {
                         return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                     }
                 })
                 .WithName(ListRelationsRouteName)
                 .WithDisplayName("List relations for a game")
                 .WithSummary("List relations for a game")
                 .WithDescription("Returns all relations associated with the specified game.")
                 .Produces<List<RelationListItem>>()
                 .RequireAuthorization(ReadPolicyName);

        relations.MapGet("/{id:guid}", async (
                     [Description("The ID of the game")] Guid gameId,
                     [Description("The ID of the relation")]
                     Guid id,
                     IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     try
                     {
                         return Results.Ok(await gameDataRepository.GetRelationByIdAsync(gameId, id, cancellationToken).ConfigureAwait(false));
                     }
                     catch (KeyNotFoundException keyNotFoundException)
                     {
                         return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                     }
                 })
                 .WithName(GetRelationRouteName)
                 .WithDisplayName("Get a relation")
                 .WithSummary("Get a relation")
                 .WithDescription("Returns a relation belonging to a specific game.")
                 .Produces<Relation>()
                 .RequireAuthorization(ReadPolicyName);

        relations.MapPost("/", async (
                     [Description("The ID of the game")] Guid gameId,
                     [Description("The data of the relation to be created")]
                     CreateRelation relation,
                     IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     try
                     {
                         var id = await gameDataRepository.CreateRelationAsync(gameId, relation, cancellationToken).ConfigureAwait(false);
                         return Results.CreatedAtRoute(GetRelationRouteName, new { gameId, id }, id);
                     }
                     catch (KeyNotFoundException keyNotFoundException)
                     {
                         return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                     }
                 })
                 .WithName(CreateRelationRouteName)
                 .WithDisplayName("Create a relation")
                 .WithSummary("Create a relation")
                 .WithDescription("Creates a new relation under the specified game.")
                 .Accepts<CreateRelation>(MediaTypeNames.Application.Json)
                 .Produces<Guid>(StatusCodes.Status201Created)
                 .WithMetadata(new CreatedLocation("The URL of the newly created relation."))
                 .ProducesValidationProblem()
                 .RequireAuthorization(WritePolicyName);

        relations.MapPatch("/{id:guid}", async (
                     [Description("The ID of the game")] Guid gameId,
                     [Description("The ID of the relation to be updated")]
                     Guid id,
                     [Description("The data of the relation to be updated")]
                     UpdateRelation relation,
                     IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     try
                     {
                         await gameDataRepository.UpdateRelationAsync(gameId, id, relation, cancellationToken).ConfigureAwait(false);
                         return Results.NoContent();
                     }
                     catch (KeyNotFoundException keyNotFoundException)
                     {
                         return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                     }
                 })
                 .WithName(UpdateRelationRouteName)
                 .WithDisplayName("Update a relation")
                 .WithSummary("Update a relation")
                 .WithDescription("Updates the specified relation under the given game.")
                 .Accepts<UpdateRelation>(MediaTypeNames.Application.Json)
                 .Produces(StatusCodes.Status204NoContent)
                 .ProducesValidationProblem()
                 .RequireAuthorization(WritePolicyName);

        relations.MapDelete("/{id:guid}", async (
                     [Description("The ID of the game")] Guid gameId,
                     [Description("The ID of the relation to be deleted")]
                     Guid id,
                     IGameDataRepository gameDataRepository, CancellationToken cancellationToken) =>
                 {
                     cancellationToken.ThrowIfCancellationRequested();
                     try
                     {
                         await gameDataRepository.DeleteRelationAsync(gameId, id, cancellationToken).ConfigureAwait(false);
                         return Results.NoContent();
                     }
                     catch (KeyNotFoundException keyNotFoundException)
                     {
                         return Results.Problem(keyNotFoundException.Message, statusCode: StatusCodes.Status404NotFound);
                     }
                 })
                 .WithName(DeleteRelationRouteName)
                 .WithDisplayName("Delete a relation")
                 .WithSummary("Delete a relation")
                 .WithDescription("Deletes the specified relation from the game.")
                 .Produces(StatusCodes.Status204NoContent)
                 .RequireAuthorization(WritePolicyName);

        return endpointRouteBuilder;
    }
}