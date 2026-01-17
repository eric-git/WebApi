using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using static WebApi.Common.Constants;

namespace WebApi.Common.Web;

public static class MiddlewareExtensions
{
    public static IEndpointRouteBuilder MapStatus(this IEndpointRouteBuilder endpointRouteBuilder, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var assembly = Assembly.GetEntryAssembly()!;
        var runInContainer = bool.TryParse(configuration["DOTNET_RUNNING_IN_CONTAINER"], out var value) && value;
        endpointRouteBuilder.MapGet("/", () => Results.Json(new
        {
            status = $"Running{(runInContainer ? " in container" : null)}",
            title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
            product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
            company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
            informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
            version = assembly.GetName().Version?.ToString(),
            timestamp = DateTime.UtcNow
        }, DataSerializationOptions));
        return endpointRouteBuilder;
    }

    public static IEndpointRouteBuilder MapFavIcon(this IEndpointRouteBuilder endpointRouteBuilder)
    {
        endpointRouteBuilder.MapGet("/favicon.ico", () =>
        {
            var filePath = Path.Combine(AppContext.BaseDirectory, "favicon.ico");
            return Results.File(filePath, "image/x-icon");
        });
        return endpointRouteBuilder;
    }
}