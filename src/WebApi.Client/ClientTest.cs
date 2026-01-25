using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Hosting;
using WebApi.Client.Model;
using WebApi.Client.Properties;

namespace WebApi.Client;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class ClientTest(IHostEnvironment hostEnvironment, IServiceClient serviceClient)
{
    private const string Esc = "\u001b[";
    private const string Yellow = Esc + "33m";
    private const string Reset = Esc + "0m";
    private const string Bold = Esc + "1m";
    private const string Green = Esc + "32m";

    public async Task TestAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        WriteTitle($"Runtime: {hostEnvironment.EnvironmentName}", false);

        List<(string Title, Func<PipelineContext, Task> Task)> pipeline =
        [
            ("creating a game record", async context =>
            {
                var newGame = GetNewGameData();
                context.CurrentGameId = await serviceClient.CreateGameAsync(newGame, cancellationToken).ConfigureAwait(false);
            }),
            ("getting a game record", async context =>
            {
                var gameId = context.CurrentGameId;
                context.CurrentGame = await serviceClient.GetGameAsync(gameId, cancellationToken).ConfigureAwait(false);
            }),
            ("updating a game record", async context =>
            {
                var game = context.CurrentGame!;
                var updateGame = new UpdateGame
                {
                    Name = $"{game.Name} - Updated",
                    Type = game.Type!,
                    PlayerName = $"{game.PlayerName} - Updated",
                    PlayerHealth = game.PlayerHealth
                };
                var gameId = context.CurrentGameId;
                await serviceClient.UpdateGameAsync(gameId, updateGame, cancellationToken).ConfigureAwait(false);
            }),
            ("listing game records", async _ => { await serviceClient.ListGamesAsync(cancellationToken).ConfigureAwait(false); }),
            ("creating new relations", async context =>
            {
                var newRelations = GetNewRelationData();
                var gameId = context.CurrentGameId;
                foreach (var newRelation in newRelations)
                {
                    var created = await serviceClient.CreateRelationAsync(gameId, newRelation, cancellationToken).ConfigureAwait(false);
                    if (context.CurrentRelationId == Guid.Empty)
                    {
                        context.CurrentRelationId = created;
                    }
                }
            }),
            ("getting a relation record", async context =>
            {
                var gameId = context.CurrentGameId;
                var relationId = context.CurrentRelationId;
                context.CurrentRelation = await serviceClient.GetRelationAsync(gameId, relationId, cancellationToken).ConfigureAwait(false);
            }),
            ("updating a relation", async context =>
            {
                var relation = context.CurrentRelation!;
                var updateRelation = new UpdateRelation
                {
                    Name = $"{relation.Name} - updated",
                    Type = relation.Type!,
                    Attributes = relation.Attributes
                };
                var gameId = context.CurrentGameId;
                var relationId = context.CurrentRelationId;
                await serviceClient.UpdateRelationAsync(gameId, relationId, updateRelation, cancellationToken).ConfigureAwait(false);
            }),
            ("listing relation records", async context =>
            {
                var gameId = context.CurrentGameId;
                await serviceClient.ListRelationsAsync(gameId, cancellationToken).ConfigureAwait(false);
            }),
            ("deleting a relation record", async context =>
            {
                var gameId = context.CurrentGameId;
                var relationId = context.CurrentRelationId;
                await serviceClient.DeleteRelationAsync(gameId, relationId, cancellationToken).ConfigureAwait(false);
            }),
            ("deleting a game record", async context =>
            {
                var gameId = context.CurrentGameId;
                await serviceClient.DeleteGameAsync(gameId, cancellationToken).ConfigureAwait(false);
            })
        ];
        PipelineContext pipelineContext = new();
        var total = pipeline.Count;
        for (var counter = 0; counter < total; counter++)
        {
            var pipelineData = pipeline[counter];
            WriteTitle($"({counter + 1} of {total}) Testing {pipelineData.Title}...");
            await pipelineData.Task(pipelineContext).ConfigureAwait(false);
        }

        WriteTitle("All done.", false);
    }

    private static void WriteTitle(string text, bool wait = true)
    {
        WriteText(text, $"{Bold}{Green}", wait);
    }

    private static void WriteText(string text, string settings, bool wait)
    {
        Console.WriteLine($"{settings}{text}{Reset}");
        if (!wait)
        {
            return;
        }

        Console.WriteLine($"{Yellow}{Resources.PressAnyKey}{Reset}");
        Console.ReadKey();
    }

    private static CreateGame GetNewGameData()
    {
        CreateGame game = new()
        {
            Type = "game",
            Name = "Mario Kart 8 Deluxe",
            PlayerName = "Mario",
            PlayerHealth = 100
        };
        return game;
    }

    private static List<CreateRelation> GetNewRelationData()
    {
        List<CreateRelation> relations =
        [
            new()
            {
                Type = "Quest",
                Name = "Win Rainbow Road",
                Attributes = new Dictionary<string, string>
                {
                    { "status", "In Progress" },
                    { "reward", "Gold Trophy" }
                }
            },
            new()
            {
                Type = "Quest",
                Name = "Collect 50 Coins",
                Attributes = new Dictionary<string, string>
                {
                    { "status", "Completed" },
                    { "reward", "Speed Boost" }
                }
            },
            new()
            {
                Type = "Equipment",
                Name = "Kart Setup",
                Attributes = new Dictionary<string, string>
                {
                    { "vehicle", "Standard Kart" },
                    { "tires", "Monster Tires" },
                    { "glider", "Super Glider" }
                }
            }
        ];
        return relations;
    }
}