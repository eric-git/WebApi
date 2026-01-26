using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using WebApi.Common.Logging;
using WebApi.Common.Web;
using WebApi.Common.Web.Logging;
using WebApi.Issuer;
using WebApi.Issuer.DataAccess;
using static WebApi.Common.Web.ErrorHandlingExtensions;

[assembly: SuppressMessage("Design", "CA1515", Justification = "Top-level Program generates a public class; safe to ignore")]

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
#pragma warning disable CA1308
            var password = configuration[$"connection-{connectionStringName.ToLowerInvariant()}.password"];
#pragma warning restore CA1308
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

builder.Services.AddTransient(typeof(IHttpLoggingHandler<>), typeof(HttpLoggingHandler<>));

var app = builder.Build();
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseExceptionHandler(applicationBuilder => { applicationBuilder.Run(HandleExceptionAsync); });

app.MapCommonRoot();
app.MapConnect();

app.Run();