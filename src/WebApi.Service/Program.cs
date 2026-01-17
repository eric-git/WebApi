using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Npgsql;
using WebApi.Common.Handler;
using WebApi.Common.Logging;
using WebApi.Common.Web;
using WebApi.Common.Web.Logging;
using WebApi.Service;
using WebApi.Service.DataAccess;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

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
#pragma warning disable CA1308
            var password = configuration[$"connection-{connectionStringName.ToLowerInvariant()}.password"];
#pragma warning restore CA1308
            NpgsqlConnectionStringBuilder npgsqlConnectionStringBuilder = new(connectionString)
            {
                Password = password
            };
            dbContextOptionsBuilder.UseNpgsql(npgsqlConnectionStringBuilder.ConnectionString);
        });
        builder.Services.AddScoped<IGameDataRepository, PostgresGameDataRepository>();
        break;
    default:
        throw new InvalidOperationException($"Unsupported persistence mode: {persistenceMode}");
}

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, jwtBearerOptions =>
    {
        var configurationManager = builder.Configuration;
        jwtBearerOptions.Authority = configurationManager["ISSUER_BASE_URL"];
        jwtBearerOptions.Audience = configurationManager["API_ID"];
        jwtBearerOptions.TokenValidationParameters = new TokenValidationParameters
        {
            NameClaimType = JwtRegisteredClaimNames.Name,
            AuthenticationType = JwtBearerDefaults.AuthenticationScheme
        };
        jwtBearerOptions.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<JwtBearerEvents>>();
                JwtBearerLog.AuthenticationFailed(logger, context.Exception);
                return Task.CompletedTask;
            },

            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<JwtBearerEvents>>();
                JwtBearerLog.ChallengeTriggered(logger, context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            },

            OnForbidden = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<JwtBearerEvents>>();
                JwtBearerLog.Forbidden(logger, context.HttpContext.User.Identity?.Name);
                return Task.CompletedTask;
            },

            OnMessageReceived = context =>
            {
                var header = context.Request.Headers[HeaderNames.Authorization].ToString();
                if (AuthenticationHeaderValue.TryParse(header, out var auth) &&
                    auth.Scheme.Equals(JwtBearerDefaults.AuthenticationScheme, StringComparison.OrdinalIgnoreCase))
                {
                    context.Token = auth.Parameter;
                }

                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<JwtBearerEvents>>();
                JwtBearerLog.MessageReceived(logger, context.Token);
                return Task.CompletedTask;
            },

            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<JwtBearerEvents>>();
                JwtBearerLog.TokenValidated(logger, context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            }
        };
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
    builder.Services.AddSingleton<CertificateHandler>();
    builder.Services.AddSingleton<IConfigureOptions<JwtBearerOptions>, JwtBearerOptionsConfigure>();
}

builder.Services.AddTransient(typeof(IHttpLoggingHandler<>), typeof(HttpLoggingHandler<>));

var app = builder.Build();
app.UseMiddleware<CorrelationMiddleware>();
app.UseMiddleware<LoggingMiddleware>();
app.UseAuthentication();
app.UseAuthorization();

var configuration = app.Services.GetRequiredService<IConfiguration>();
app.MapFavIcon();
app.MapStatus(configuration);
app.MapGame();

app.Run();