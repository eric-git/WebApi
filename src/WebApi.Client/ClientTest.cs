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

    public async Task TestAsync()
    {
        WriteTitle($"Runtime: {hostEnvironment.EnvironmentName}", false);
        WriteTitle("(1/5) Test creating a record...");
        var newGame = GetNewGameData();
        var id = await serviceClient.CreateGameAsync(newGame).ConfigureAwait(false);

        WriteTitle("(2/5) Test getting a single record...");
        var game = await serviceClient.GetGameAsync(id!).ConfigureAwait(false);

        WriteTitle("(3/5) Test updating a record...");
        game!.Name = $"{game.Name} - Updated";
        await serviceClient.UpdateGameAsync(game).ConfigureAwait(false);

        WriteTitle("(4/5) Test deleting a record...");
        await serviceClient.DeleteGameAsync(id!).ConfigureAwait(false);

        WriteTitle("(5/5) Test listing records...");
        await serviceClient.ListGamesAsync().ConfigureAwait(false);

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

    private static Game GetNewGameData()
    {
        Game game = new()
        {
            Type = "game",
            Name = "Mario Kart 8 Deluxe",
            PlayerName = "Mario",
            PlayerHealth = 100,
            Relations =
            [
                new Relation
                {
                    Type = "Quest",
                    Name = "Win Rainbow Road",
                    Attributes = new Dictionary<string, string>
                    {
                        { "status", "In Progress" },
                        { "reward", "Gold Trophy" }
                    }
                },
                new Relation
                {
                    Type = "Quest",
                    Name = "Collect 50 Coins",
                    Attributes = new Dictionary<string, string>
                    {
                        { "status", "Completed" },
                        { "reward", "Speed Boost" }
                    }
                },
                new Relation
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
            ]
        };

        return game;
    }
}