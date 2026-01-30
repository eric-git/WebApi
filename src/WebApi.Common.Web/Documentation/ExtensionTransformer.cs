using System.Text.Json.Nodes;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Common.Web.Documentation;

public class ExtensionTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        document.Info.Extensions ??= new Dictionary<string, IOpenApiExtension>();
        document.Info.Extensions["x-logo"] = new JsonNodeExtension(
            new JsonObject
            {
                ["url"] = "/docs/logo.png",
                ["altText"] = "Project logo"
            }
        );
        return Task.CompletedTask;
    }
}