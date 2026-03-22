using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace WebApi.Common.Web.Validation;

public sealed class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);
        var argument = context.Arguments.FirstOrDefault(a => a is not null);
        if (argument is null)
        {
            return await next(context).ConfigureAwait(false);
        }

        ValidationContext validationContext = new(argument);
        List<ValidationResult> results = [];
        if (Validator.TryValidateObject(argument, validationContext, results, true))
        {
            return await next(context).ConfigureAwait(false);
        }

        var errors = results
                     .GroupBy(r => r.MemberNames.FirstOrDefault() ?? string.Empty)
                     .ToDictionary(g => g.Key, g => g.Select(r => r.ErrorMessage!).ToArray());
        return Results.ValidationProblem(errors);
    }
}