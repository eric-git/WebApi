using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Common.Web.Documentation;

public sealed class TagDescriptionTransformer : IOpenApiDocumentTransformer
{
    public Task TransformAsync(OpenApiDocument document, OpenApiDocumentTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(context);
        var tagDescriptions = context.DescriptionGroups
            .SelectMany(g => g.Items)
            .SelectMany(d => d.ActionDescriptor.EndpointMetadata)
            .OfType<TagDescription>()
            .Distinct()
            .ToList();
        foreach (var tag in document.Tags!)
        {
            var description = tagDescriptions
                .FirstOrDefault(td => td.Name.Equals(tag.Name, StringComparison.OrdinalIgnoreCase))
                ?.Description;
            if (!string.IsNullOrWhiteSpace(description))
            {
                tag.Description = description;
            }
        }

        return Task.CompletedTask;
    }
}