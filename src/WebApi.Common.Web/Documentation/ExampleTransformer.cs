using System.Globalization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi;

namespace WebApi.Common.Web.Documentation;

public sealed class ExampleTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(OpenApiOperation operation, OpenApiOperationTransformerContext context, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);
        ApplyRequestExample(operation, context);
        ApplyResponseExample(operation, context);
        return Task.CompletedTask;
    }

    private static void ApplyRequestExample(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        var requestType = context.Description.ParameterDescriptions
            .Where(p => p.Source.Id == "Body")
            .Select(p => p.Type)
            .FirstOrDefault();
        if (requestType is null || operation.RequestBody is null)
        {
            return;
        }

        var exampleFactory = context.ApplicationServices.GetRequiredService<ExampleFactory>();
        var example = exampleFactory.Resolve(requestType);
        if (example is null)
        {
            return;
        }

        foreach (var content in operation.RequestBody.Content!.Values)
        {
            content.Example = example;
        }
    }

    private static void ApplyResponseExample(OpenApiOperation operation, OpenApiOperationTransformerContext context)
    {
        var responseType = context.Description.SupportedResponseTypes
            .Select(r => r.Type)
            .FirstOrDefault(t =>
                t is not null &&
                t != typeof(void) &&
                t != typeof(ProblemDetails) &&
                t != typeof(HttpValidationProblemDetails) &&
                t != typeof(ValidationProblemDetails));
        if (responseType is null)
        {
            return;
        }

        var exampleFactory = context.ApplicationServices.GetRequiredService<ExampleFactory>();
        var example = exampleFactory.Resolve(responseType);
        if (example is null)
        {
            return;
        }

        foreach (var (statusCode, response) in operation.Responses!)
        {
            var declaredType = context.Description.SupportedResponseTypes
                .FirstOrDefault(r => r.StatusCode.ToString(CultureInfo.InvariantCulture) == statusCode)?
                .Type;
            if (declaredType != responseType || !response.Content!.TryGetValue("application/json", out var jsonContent))
            {
                continue;
            }

            jsonContent.Example = example;
        }
    }
}