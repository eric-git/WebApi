using System.Diagnostics.CodeAnalysis;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using static WebApi.Common.Metadata.AssemblyMetadata;
using static WebApi.Common.Metadata.EmbeddedResource;

namespace WebApi.Service.Documentation;

[SuppressMessage("Performance", "CA1812", Justification = "Instantiated by DI container")]
internal sealed class DocumentInfoTransformer(IConfiguration configuration) : Common.Web.Documentation.DocumentInfoTransformer
{
    public override async Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        await base.TransformAsync(document, context, cancellationToken).ConfigureAwait(false);
        var description = Read("description.md");
        document.Info.Version = "v1";
        document.Info.Description = description.Replace("{ISSUER_BASE_URL}", configuration["ISSUER_BASE_URL"], StringComparison.OrdinalIgnoreCase);
        document.Info.Summary = Get("ProjectSummary");
    }
}