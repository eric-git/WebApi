using System.Reflection;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Common.Web.Documentation;

public abstract class DocumentInfoTransformer : IOpenApiDocumentTransformer
{
    public virtual Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);
        var assembly = Assembly.GetEntryAssembly()!;
        document.Info.Title = assembly.GetCustomAttribute<AssemblyTitleAttribute>()?.Title;
        document.Info.Contact = new OpenApiContact
        {
            Name = "Eric Wu",
            Email = "wu_yuqing@hotmail.com"
        };
        document.Info.License = new OpenApiLicense
        {
            Name = "MIT",
            Url = new Uri("https://github.com/eric-git/WebApi?tab=MIT-1-ov-file")
        };
        document.Info.TermsOfService = new Uri("https://github.com/eric-git/WebApi?tab=security-ov-file");
        return Task.CompletedTask;
    }
}