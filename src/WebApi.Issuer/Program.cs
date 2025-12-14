using System.Reflection;
using WebApi.Issuer;
using static WebApi.Common.MiddlewareExtensions;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var app = builder.Build();
app.ProvideFavIcon();
app.MapStatus(
        Assembly.GetEntryAssembly()!,
        bool.TryParse(app.Configuration["DOTNET_RUNNING_IN_CONTAINER"], out var inContainer) && inContainer)
    .MapToken(
        app.Configuration["ISSUER_BASE_URL"]!,
        int.Parse(app.Configuration["TOKEN_LIFETIME_MINUTES"]!));
app.Run();