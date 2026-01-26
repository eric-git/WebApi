using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static WebApi.Common.Constants;

namespace WebApi.Common.Web;

public static class MiddlewareExtensions
{
    public const string RootRouteName = "Root";
    public const string FavIconRouteName = "FavIcon";

    public static IEndpointRouteBuilder MapCommonRoot(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        var assembly = Assembly.GetEntryAssembly()!;
        endpointRouteBuilder.MapGet("/", (IConfiguration configuration, CancellationToken cancellationToken) =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var runInContainer = bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out var value) && value;
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
                var filePath = Path.Combine(AppContext.BaseDirectory, "favicon.ico");
                return Results.File(filePath, "image/x-icon");
            })
            .WithName(FavIconRouteName)
            .ExcludeFromDescription();
        return endpointRouteBuilder;
    }

    public static IEndpointRouteBuilder MapDocument(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/docs", async context =>
            {
                context.Response.ContentType = "text/html";
                var filePath = Path.Combine(AppContext.BaseDirectory, "redoc/index.html");
                await context.Response.SendFileAsync(filePath).ConfigureAwait(false);
            })
            .ExcludeFromDescription();
        return endpointRouteBuilder;
    }
}