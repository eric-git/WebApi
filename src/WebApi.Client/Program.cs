using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Client;
using WebApi.Client.Services;
using WebApi.Common.Handler;
using WebApi.Common.Logging;
using static WebApi.Common.SecurityExtensions;
using IdentityLogLevel = Microsoft.Identity.Client.LogLevel;

var builder = Host.CreateApplicationBuilder(args);
builder.Configuration.AddEnvironmentVariables();
var secretPath = builder.Configuration["SECRET_PATH"];
if (!Path.IsPathRooted(secretPath!))
{
    secretPath = Path.Combine(AppContext.BaseDirectory, secretPath!);
}

builder.Configuration.AddKeyPerFile(secretPath);
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

builder.Services.AddSingleton<IConfidentialClientApplication>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var msalHttpClientFactory = serviceProvider.GetRequiredService<IMsalHttpClientFactory>();
    var logger = serviceProvider.GetRequiredService<ILogger<IConfidentialClientApplication>>();
    var issuer = configuration["ISSUER_BASE_URL"]!;
    var clientId = configuration["CLIENT_ID"]!;
    var apiId = configuration["API_ID"]!;
    var keyId = configuration["KEY_ID"]!;
    return ConfidentialClientApplicationBuilder
        .Create(clientId)
        .WithOidcAuthority(issuer)
        .WithHttpClientFactory(msalHttpClientFactory)
        .WithClientAssertion(async (CancellationToken _) =>
        {
            var privateSigningKey = configuration["private-signing-key.pem"];
            var (_, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemAsync(privateSigningKey!, keyId).ConfigureAwait(false);
            var now = DateTime.UtcNow;
            SecurityTokenDescriptor securityTokenDescriptor = new()
            {
                Issuer = issuer,
                Audience = apiId,
                Claims = new Dictionary<string, object>
                {
                    [JwtRegisteredClaimNames.Sub] = clientId,
                    [JwtRegisteredClaimNames.Jti] = Guid.NewGuid().ToString("N")
                },
                NotBefore = now,
                Expires = now.AddMinutes(5),
                SigningCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.RsaSha256)
            };
            JsonWebTokenHandler jsonWebTokenHandler = new();
            var jwt = jsonWebTokenHandler.CreateToken(securityTokenDescriptor);
            return jwt;
        })
        .WithLogging((level, message, containsPii) =>
        {
            var formatted = containsPii ? $"[PII] {message}" : message;
            switch (level)
            {
                case IdentityLogLevel.Error:
                    MsalLog.Error(logger, formatted);
                    break;
                case IdentityLogLevel.Warning:
                    MsalLog.Warning(logger, formatted);
                    break;
                case IdentityLogLevel.Info:
                    MsalLog.Info(logger, formatted);
                    break;
                default:
                    MsalLog.Debug(logger, formatted);
                    break;
            }
        })
        .Build();
});

builder.Services.AddHttpClient(ServiceClient.HttpClientName, (serviceProvider, httpClient) =>
    {
        var configuration = serviceProvider.GetRequiredService<IConfiguration>();
        httpClient.BaseAddress = new Uri($"{configuration["API_BASE_URL"]}/");
        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
    })
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
        return hostEnvironment.IsDevelopment()
            ? serviceProvider.GetRequiredService<CertificateHandler>()
            : new HttpClientHandler();
    })
    .AddHttpMessageHandler<CorrelationHandler>()
    .AddHttpMessageHandler<AccessTokenHandler>()
    .AddHttpMessageHandler<LoggingHandler>();
builder.Services.AddHttpClient(MsalHttpClientFactory.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
        return hostEnvironment.IsDevelopment()
            ? serviceProvider.GetRequiredService<CertificateHandler>()
            : new HttpClientHandler();
    })
    .AddHttpMessageHandler<LoggingHandler>();

builder.Services.AddSingleton<ITokenService, MsalTokenService>();
builder.Services.AddSingleton<IMsalHttpClientFactory, MsalHttpClientFactory>();
builder.Services.AddSingleton<IServiceClient, ServiceClient>();
builder.Services.AddTransient<CorrelationHandler>();
builder.Services.AddTransient(typeof(IHttpLoggingHandler<>), typeof(HttpLoggingHandler<>));
builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.AddTransient<ClientTest>();
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddSingleton<CertificateHandler>();
}

var app = builder.Build();
await app.Services.GetRequiredService<ClientTest>().TestAsync().ConfigureAwait(false);