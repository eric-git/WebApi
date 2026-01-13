using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Npgsql;
using WebApi.Common;
using WebApi.Common.Web;
using WebApi.Service;
using WebApi.Service.DataAccess;

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
        builder.Services.AddTransient<IGameDataRepository, JsonFileGameDataRepository>();
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
        builder.Services.AddScoped<IGameDataRepository, PostgresGameDataRepository>();
        builder.Services.AddAutoMapper(_ => { }, AppDomain.CurrentDomain.GetAssemblies());
        break;
    default:
        throw new InvalidOperationException($"Unsupported persistence mode: {persistenceMode}");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(jwtBearerOptions =>
    {
        var configurationManager = builder.Configuration;
        jwtBearerOptions.Authority = configurationManager["ISSUER_BASE_URL"];
        jwtBearerOptions.Audience = configurationManager["API_ID"];
    });
builder.Services.AddAuthorization(authorizationOptions =>
{
    authorizationOptions.AddPolicy("ApiRead", authorizationPolicyBuilder =>
    {
        authorizationPolicyBuilder.RequireAuthenticatedUser();
        authorizationPolicyBuilder.RequireClaim("scope", "api.read");
    });
    authorizationOptions.AddPolicy("ApiWrite", authorizationPolicyBuilder =>
    {
        authorizationPolicyBuilder.RequireAuthenticatedUser();
        authorizationPolicyBuilder.RequireClaim("scope", "api.write");
    });
});
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<ClientCertificateHandler>();
    builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigure>();
}

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();

var configuration = app.Services.GetRequiredService<IConfiguration>();
app.MapFavIcon();
app.MapStatus(configuration);
app.MapGame();

app.Run();