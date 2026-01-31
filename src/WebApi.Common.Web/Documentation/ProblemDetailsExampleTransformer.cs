using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.OpenApi;
using static WebApi.Common.Web.ErrorHandling.Constants;

namespace WebApi.Common.Web.Documentation;

public sealed class ProblemDetailsExampleTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        foreach (var (statusCode, response) in operation.Responses!)
        {
            if (response.Content?.TryGetValue("application/problem+json", out var mediaType) == true)
            {
                mediaType.Example = CreateExample(statusCode, context.Description.RelativePath);
            }
        }

        return Task.CompletedTask;
    }

    private static JsonObject? CreateExample(string statusCode, string? path)
    {
        return statusCode switch
        {
            "400" => new JsonObject
            {
                ["type"] = JsonValue.Create($"{BaseErrorTypeUri}/{StatusCodes.Status400BadRequest}"),
                ["title"] = JsonValue.Create("One or more validation errors occurred."),
                ["status"] = JsonValue.Create(StatusCodes.Status400BadRequest),
                ["instance"] = JsonValue.Create(path),
                ["errors"] = new JsonObject
                {
                    ["fieldName"] = new JsonArray("The field is required.")
                },
                [TraceIdPropertyName] = JsonValue.Create("0HNIV5A3KTS4L:00000002")
            },
            "401" => new JsonObject
            {
                ["type"] = JsonValue.Create($"{BaseErrorTypeUri}/{StatusCodes.Status401Unauthorized}"),
                ["title"] = JsonValue.Create(ReasonPhrases.GetReasonPhrase(StatusCodes.Status401Unauthorized)),
                ["status"] = JsonValue.Create(StatusCodes.Status401Unauthorized),
                ["detail"] = JsonValue.Create("Authentication is required to access this resource."),
                ["instance"] = JsonValue.Create(path),
                [TraceIdPropertyName] = JsonValue.Create("0HNIV5A3KTS4L:00000001")
            },

            "403" => new JsonObject
            {
                ["type"] = JsonValue.Create($"{BaseErrorTypeUri}/{StatusCodes.Status403Forbidden}"),
                ["title"] = JsonValue.Create(ReasonPhrases.GetReasonPhrase(StatusCodes.Status403Forbidden)),
                ["status"] = JsonValue.Create(StatusCodes.Status403Forbidden),
                ["detail"] = JsonValue.Create("You do not have permission to access this resource."),
                ["instance"] = JsonValue.Create(path),
                [TraceIdPropertyName] = JsonValue.Create("0HNIV5A3KTS4L:00000003")
            },

            "404" => new JsonObject
            {
                ["type"] = JsonValue.Create($"{BaseErrorTypeUri}/{StatusCodes.Status404NotFound}"),
                ["title"] = JsonValue.Create(ReasonPhrases.GetReasonPhrase(StatusCodes.Status404NotFound)),
                ["status"] = JsonValue.Create(StatusCodes.Status404NotFound),
                ["detail"] = JsonValue.Create("The requested resource was not found."),
                ["instance"] = JsonValue.Create(path),
                [TraceIdPropertyName] = JsonValue.Create("0HNIV5A3KTS4L:00000004")
            },

            "500" => new JsonObject
            {
                ["type"] = JsonValue.Create($"{BaseErrorTypeUri}/{StatusCodes.Status500InternalServerError}"),
                ["title"] = JsonValue.Create(ReasonPhrases.GetReasonPhrase(StatusCodes.Status500InternalServerError)),
                ["status"] = JsonValue.Create(StatusCodes.Status500InternalServerError),
                ["detail"] = JsonValue.Create("An unexpected error occurred."),
                ["instance"] = JsonValue.Create(path),
                [TraceIdPropertyName] = JsonValue.Create("0HNIV5A3KTS4L:00000005")
            },

            _ => null
        };
    }
}