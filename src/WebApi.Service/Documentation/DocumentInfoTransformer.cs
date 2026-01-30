using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Service.Documentation;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class DocumentInfoTransformer : Common.Web.Documentation.DocumentInfoTransformer
{
    public override async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        await base.TransformAsync(document, context, cancellationToken).ConfigureAwait(false);
        document.Info.Version = "v1";
        document.Info.Description = "Sample Web API service for processing game records.";
        document.Info.Summary = "Game Web API";
    }
}