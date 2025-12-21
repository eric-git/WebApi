using System.Net.Http.Headers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using WebApi.Client;
using WebApi.Client.Handlers;
using WebApi.Client.Services;
using WebApi.Common;
using static WebApi.Common.SecurityExtensions;
using static WebApi.Common.Constants;

var builder = Host.CreateApplicationBuilder(args);
if (builder.Environment.IsDevelopment())
{
    CertificateStore.Load();
}

builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
builder.Logging.AddConsole();

builder.Services.AddSingleton<IConfidentialClientApplication>(serviceProvider =>
{
    var configuration = serviceProvider.GetRequiredService<IConfiguration>();
    var issuer = configuration["ISSUER_BASE_URL"]!;
    var clientId = configuration["CLIENT_ID"]!;
    var apiId = configuration["API_ID"]!;
    return ConfidentialClientApplicationBuilder
        .Create(clientId)
        .WithOidcAuthority(issuer)
        .WithHttpClientFactory(serviceProvider.GetRequiredService<IMsalHttpClientFactory>())
        .WithClientAssertion(async (CancellationToken _) =>
        {
            var fileName = Path.Combine(KeyStoreRootPath, "private.pem");
            var (_, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync(fileName);
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
            ? new HttpClientHandler { ServerCertificateCustomValidationCallback = CertificateValidationCallback }
            : new HttpClientHandler();
    })
    .AddHttpMessageHandler<AccessTokenHandler>()
    .AddHttpMessageHandler<LoggingHandler>();
builder.Services.AddHttpClient(MsalHttpClientFactory.HttpClientName)
    .ConfigurePrimaryHttpMessageHandler(serviceProvider =>
    {
        var hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
        return hostEnvironment.IsDevelopment()
            ? new HttpClientHandler { ServerCertificateCustomValidationCallback = CertificateValidationCallback }
            : new HttpClientHandler();
    });

builder.Services.AddSingleton<ITokenService, MsalTokenService>();
builder.Services.AddSingleton<IMsalHttpClientFactory, MsalHttpClientFactory>();
builder.Services.AddSingleton<IServiceClient, ServiceClient>();
builder.Services.AddTransient<LoggingHandler>();
builder.Services.AddTransient<AccessTokenHandler>();
builder.Services.AddTransient<ClientTest>();

var app = builder.Build();
await app.Services.GetRequiredService<ClientTest>().TestAsync();