using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace WebApi.Common.Web.Documentation;

public class CreatedLocationHeaderTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        if (operation.Responses is null || !operation.Responses.TryGetValue("201", out var response))
        {
            return Task.CompletedTask;
        }

        var headers = response.Headers;
        if (headers is null)
        {
            var headersProperty = typeof(OpenApiResponse).GetProperty(nameof(OpenApiResponse.Headers));
            if (headersProperty is null)
            {
                return Task.CompletedTask;
            }

            headers = new Dictionary<string, IOpenApiHeader>();
            headersProperty.SetValue(response, headers);
        }
        else if (headers.ContainsKey("Location"))
        {
            return Task.CompletedTask;
        }

        var createdLocation = context.Description.ActionDescriptor.EndpointMetadata
                                     .OfType<CreatedLocation>()
                                     .FirstOrDefault();
        if (createdLocation is null)
        {
            return Task.CompletedTask;
        }

        headers.Add("Location", new OpenApiHeader
        {
            Description = string.IsNullOrWhiteSpace(createdLocation.Description) ? "The URL of the newly created resource." : createdLocation.Description,
            Schema = new OpenApiSchema
            {
                Type = JsonSchemaType.String,
                Format = "uri"
            }
        });
        return Task.CompletedTask;
    }
}