using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using static WebApi.Common.Constants;

namespace WebApi.Common.Web;

public static class ApiEndpoints
{
    public const string RootRouteName = "Root";
    public const string FavIconRouteName = "FavIcon";

    public static IEndpointRouteBuilder MapCommonEndpoints(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/", (IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var runInContainer = bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out var value) && value;
                var assembly = Assembly.GetEntryAssembly()!;
                return Results.Json(new
                {
                    status = $"Running{(runInContainer ? " in container" : null)}",
                    title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
                    product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                    company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
                    informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                    version = assembly.GetName().Version?.ToString(),
                    timestamp = DateTime.UtcNow
                }, DataSerializationOptions);
            })
            .WithName(RootRouteName)
            .ExcludeFromDescription();

        endpointRouteBuilder.MapGet("/favicon.ico", (CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var env = endpointRouteBuilder.ServiceProvider.GetRequiredService<IHostEnvironment>();
                var filePath = Path.Combine(env.ContentRootPath, "favicon.ico");
                return Results.File(filePath, "image/x-icon");
            })
            .WithName(FavIconRouteName)
            .ExcludeFromDescription();
        return endpointRouteBuilder;
    }

    public static IApplicationBuilder MapDocument(this IApplicationBuilder applicationBuilder)
    {
        ArgumentNullException.ThrowIfNull(applicationBuilder);
        var hostEnvironment = applicationBuilder.ApplicationServices.GetRequiredService<IHostEnvironment>();
        var docPath = Path.Combine(hostEnvironment.ContentRootPath, "docs");
        applicationBuilder.UseFileServer(new FileServerOptions
        {
            FileProvider = new PhysicalFileProvider(docPath),
            RequestPath = "/docs",
            EnableDefaultFiles = true
        });
        return applicationBuilder;
    }
}