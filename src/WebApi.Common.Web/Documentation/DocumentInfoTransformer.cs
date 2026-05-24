using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using static WebApi.Common.Metadata.AssemblyMetadata;

namespace WebApi.Common.Web.Documentation;

public abstract class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    public virtual Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);
        var assembly = Assembly.GetEntryAssembly()!;
        document.Info.Title = GetTitle();
        document.Info.Contact = new OpenApiContact
        {
            Name = Get("ProjectContactName"),
            Email = Get("ProjectContactEmail"),
            Url = new Uri(Get("ProjectContactUrl")!)
        };
        document.Info.License = new OpenApiLicense
        {
            Name = Get("ProjectLicenseName"),
            Url = new Uri(Get("ProjectLicenseUrl")!)
        };
        document.Info.TermsOfService = new Uri(Get("ProjectTermsUrl")!);
        return Task.CompletedTask;
    }
}