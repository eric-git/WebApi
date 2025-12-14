using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace WebApi.Common;

public static class MiddlewareExtensions
{
    extension(IEndpointRouteBuilder endpointRouteBuilder)
    {
        public IEndpointRouteBuilder MapStatus(Assembly assembly, bool runInContainer)
        {
            ArgumentNullException.ThrowIfNull(assembly);
            endpointRouteBuilder.MapGet("/", () => Results.Json(new
            {
                status = $"Running{(runInContainer ? " in container" : null)}",
                title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title,
                product = assembly.GetCustomAttribute<AssemblyProductAttribute>()?.Product,
                company = assembly.GetCustomAttribute<AssemblyCompanyAttribute>()?.Company,
                informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion,
                version = assembly.GetName().Version?.ToString(),
                timestamp = DateTime.UtcNow
            }, new JsonSerializerOptions { WriteIndented = true }));
            return endpointRouteBuilder;
        }

        public IEndpointRouteBuilder ProvideFavIcon()
        {
            endpointRouteBuilder.MapGet("/favicon.ico", async context =>
            {
                context.Response.ContentType = "image/x-icon";
                await context.Response.SendFileAsync(Path.Combine(AppContext.BaseDirectory, "favicon.ico"));
            });
            return endpointRouteBuilder;
        }
    }
}