using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Client;
using Microsoft.IdentityModel.Tokens;
using WebApi.Client;
using WebApi.Client.Handlers;
using WebApi.Client.Services;
using static WebApi.Common.SecurityExtensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.Sources.Clear();
        config.AddEnvironmentVariables();
        config.AddCommandLine(args);
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddSingleton<IConfidentialClientApplication>(provider =>
        {
            var issuer = context.Configuration["ISSUER_BASE_URL"]!;
            var clientId = context.Configuration["CLIENT_ID"]!;
            var confidentialClientApplication = ConfidentialClientApplicationBuilder
                .Create(clientId)
                .WithOidcAuthority(issuer)
                .WithHttpClientFactory(provider.GetRequiredService<IMsalHttpClientFactory>())
                .WithClientAssertion(async (CancellationToken _) =>
                {
                    var (_, rsaSecurityKey) = await CreateRsaSecurityKeyFromPemFileAsync("./signing/private.pem");
                    SigningCredentials signingCredentials = new(rsaSecurityKey, SecurityAlgorithms.RsaSha256);
                    var now = DateTime.UtcNow;
                    JwtSecurityToken jwtSecurityToken = new(
                        issuer,
                        context.Configuration["API_ID"]!,
                        [
                            new Claim(JwtRegisteredClaimNames.Sub, clientId),
                            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
                        ],
                        now,
                        now.AddMinutes(5),
                        signingCredentials
                    );
                    var clientAssertion = new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
                    return clientAssertion;
                })
                .Build();
            return confidentialClientApplication;
        });
        services.AddScoped<ITokenService, MsalTokenService>();
        services.AddScoped<IMsalHttpClientFactory, MsalHttpClientFactory>();
        services.AddScoped<IServiceClient, ServiceClient>();
        services.AddScoped<LoggingHandler>();
        services.AddScoped<AccessTokenHandler>();
        services.AddHttpClient(ServiceClient.HttpClientName, client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri(context.Configuration["API_BASE_URL"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = CertificateValidationCallback
            })
            .AddHttpMessageHandler<AccessTokenHandler>()
            .AddHttpMessageHandler<LoggingHandler>();
        services.AddHttpClient(MsalHttpClientFactory.HttpClientName)
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = CertificateValidationCallback
            });
        services.AddScoped<ClientTest>();
    })
    .Build();

var clientTest = host.Services.GetRequiredService<ClientTest>();
await clientTest.TestAsync();