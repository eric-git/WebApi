using System.Net.Http.Headers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebApi.Client;
using WebApi.Common;
using static WebApi.Common.SecurityExtensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureAppConfiguration((context, config) =>
    {
        config.Sources.Clear();
        var environment = context.HostingEnvironment.EnvironmentName;
        config.AddJsonFile("appsettings.json", false, true)
            .AddJsonFile($"appsettings.{environment}.json", true, true)
            .AddEnvironmentVariables()
            .AddCommandLine(args);
    })
    .ConfigureLogging((context, logging) =>
    {
        logging.AddConfiguration(context.Configuration.GetSection("Logging"));
        logging.AddConsole();
    })
    .ConfigureServices((context, services) =>
    {
        services.AddMemoryCache()
            .AddScoped<IServiceClient, ServiceClient>()
            .AddScoped<ITokenService, AccessTokenService>(provider =>
            {
                AccessTokenService accessTokenService = new(
                    context.Configuration["API_ID"]!,
                    context.Configuration["CLIENT_ID"]!,
                    provider.GetRequiredService<IHttpClientFactory>(),
                    provider.GetRequiredService<IMemoryCache>());
                return accessTokenService;
            })
            .AddScoped<AccessTokenHandler>()
            .AddTransient<LoggingHandler>()
            .AddScoped<ClientTest>();
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
        services.AddHttpClient(AccessTokenService.HttpClientName, client =>
            {
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                client.BaseAddress = new Uri(context.Configuration["ISSUER_BASE_URL"]!);
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = CertificateValidationCallback
            })
            .AddHttpMessageHandler<LoggingHandler>();
    })
    .Build();

var clientTest = host.Services.GetRequiredService<ClientTest>();
await clientTest.TestAsync();