using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApi.Common.Web;
using WebApi.Issuer;
using WebApi.Issuer.DataAccess;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var secretPath = builder.Configuration["SECRET_PATH"];
if (!Path.IsPathRooted(secretPath!))
{
    secretPath = Path.Combine(AppContext.BaseDirectory, secretPath!);
}

builder.Configuration.AddKeyPerFile(secretPath);
var persistenceMode = builder.Configuration["Persistence:Mode"];
switch (persistenceMode)
{
    case "json":
        builder.Services.AddTransient<ISettingsDataRepository, JsonFileSettingsDataRepository>();
        break;
    case "postgres":
        builder.Services.AddDbContext<AppDbContext>((serviceCollection, dbContextOptionsBuilder) =>
        {
            const string connectionStringName = "Default";
            var configuration = serviceCollection.GetRequiredService<IConfiguration>();
            var connectionString = configuration.GetConnectionString(connectionStringName);
            var password = configuration[$"connection-{connectionStringName.ToLower()}.password"];
            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new(connectionString)
            {
                Password = password
            };

            dbContextOptionsBuilder.UseNpgsql(npgsqlConnectionStringBuilder.ConnectionString);
        });
        builder.Services.AddScoped<ISettingsDataRepository, PostgresSettingsDataRepository>();
        break;
    default:
        throw new InvalidOperationException($"Unsupported persistence mode: {persistenceMode}");
}

var app = builder.Build();

var configuration = app.Services.GetRequiredService<IConfiguration>();
var issuerBaseUrl = configuration["ISSUER_BASE_URL"]!;
var tokenLiftTime = configuration["TOKEN_LIFETIME_MINUTES"]!;
app.MapFavIcon();
app.MapStatus(configuration);
app.MapIssuer(issuerBaseUrl, int.Parse(tokenLiftTime));

app.Run();