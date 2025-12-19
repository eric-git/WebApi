using Microsoft.Extensions.Hosting;
using WebApi.Client.Model;

namespace WebApi.Client;

public class ClientTest(IHostEnvironment hostEnvironment, IServiceClient serviceClient)
{
    public async Task TestAsync()
    {
        WriteTitle($"Runtime: {hostEnvironment.EnvironmentName}", false);
        WriteTitle("(1/5) Test creating a record...");
        var newGame = GetNewGameData();
        var id = await serviceClient.CreateGameAsync(newGame);

        WriteTitle("(2/5) Test getting a single record...");
        var game = await serviceClient.GetGameAsync(id!);

        WriteTitle("(3/5) Test updating a record...");
        game!.Name = $"{game.Name} - Updated";
        await serviceClient.UpdateGameAsync(game);

        WriteTitle("(4/5) Test deleting a record...");
        await serviceClient.DeleteGameAsync(id!);

        WriteTitle("(5/5) Test listing records...");
        await serviceClient.ListGamesAsync();

        WriteTitle("All done.", false);
    }

    private static void WriteTitle(string text, bool wait = true)
    {
        WriteText(text, "\e[1m\e[32m", wait);
    }

    private static void WriteText(string text, string settings, bool wait)
    {
        Console.WriteLine($"{settings}{text}\e[0m");
        if (!wait)
        {
            return;
        }

        Console.WriteLine("\e[33mPress any key to continue, or terminate...\e[0m");
        Console.ReadKey();
    }

    private static Game GetNewGameData()
    {
        Game game = new()
        {
            Type = "Game",
            Name = "Mario Kart 8 Deluxe",
            Attributes = new Attributes
            {
                Player = new Player
                {
                    Name = "Mario",
                    Health = 100
                }
            },
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