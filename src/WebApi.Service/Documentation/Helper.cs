using WebApi.Common.Web.Documentation;
using WebApi.Service.Model;

namespace WebApi.Service.Documentation;

internal static class Helper
{
    public static void RegisterExamples(this IServiceProvider serviceProvider)
    {
        var factory = serviceProvider.GetRequiredService<ExampleFactory>();
        factory.Register(() =>
        {
            CreateGame game = new()
            {
                Type = "game",
                Name = "Mario Kart 8 Deluxe",
                PlayerName = "Mario",
                PlayerHealth = 100
            };
            return game;
        });

        factory.Register(() =>
        {
            CreateRelation relation = new()
            {
                Type = "Quest",
                Name = "Win Rainbow Road",
                Attributes = new Dictionary<string, string>
                {
                    { "status", "In Progress" },
                    { "reward", "Gold Trophy" }
                }
            };
            return relation;
        });

        factory.Register(() =>
        {
            List<Relation> relations =
            [
                new()
                {
                    Id = Guid.NewGuid(),
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
                    Id = Guid.NewGuid(),
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
                    Id = Guid.NewGuid(),
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
            Game game = new()
            {
                Id = Guid.NewGuid(),
                Type = "game",
                Name = "Mario Kart 8 Deluxe",
                PlayerName = "Mario",
                PlayerHealth = 100,
                Relations = relations
            };
            return game;
        });

        factory.Register(() =>
        {
            GameListItem game = new()
            {
                Id = Guid.NewGuid(),
                Type = "game",
                Name = "Mario Kart 8 Deluxe",
                PlayerName = "Mario",
                PlayerHealth = 100
            };
            return game;
        });

        factory.Register(() =>
        {
            Relation relation = new()
            {
                Id = Guid.NewGuid(),
                Type = "Quest",
                Name = "Win Rainbow Road",
                Attributes = new Dictionary<string, string>
                {
                    { "status", "In Progress" },
                    { "reward", "Gold Trophy" }
                }
            };
            return relation;
        });

        factory.Register(() =>
        {
            RelationListItem relation = new()
            {
                Id = Guid.NewGuid(),
                Type = "Quest",
                Name = "Win Rainbow Road"
            };
            return relation;
        });

        factory.Register(() =>
        {
            UpdateGame game = new()
            {
                Type = "game",
                Name = "Mario Kart 8 Deluxe",
                PlayerName = "Mario",
                PlayerHealth = 100
            };
            return game;
        });

        factory.Register(() =>
        {
            UpdateRelation relation = new()
            {
                Type = "Quest",
                Name = "Win Rainbow Road",
                Attributes = new Dictionary<string, string>
                {
                    { "status", "In Progress" },
                    { "reward", "Gold Trophy" }
                }
            };
            return relation;
        });
    }
}