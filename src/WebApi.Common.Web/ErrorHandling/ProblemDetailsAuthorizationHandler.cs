using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Policy;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static WebApi.Common.Web.ErrorHandling.Constants;

namespace WebApi.Common.Web.ErrorHandling;

public sealed class ProblemDetailsAuthorizationHandler(IProblemDetailsService problemDetailsService) : IAuthorizationMiddlewareResultHandler
{
    public async Task HandleAsync(RequestDelegate next, HttpContext context, AuthorizationPolicy policy, PolicyAuthorizationResult authorizeResult)
    {
        ArgumentNullException.ThrowIfNull(next);
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(authorizeResult);
        if (authorizeResult.Succeeded)
        {
            await next(context).ConfigureAwait(false);
            return;
        }

        ProblemDetails problemDetails = new()
        {
            Status = authorizeResult.Forbidden
                ? StatusCodes.Status403Forbidden
                : StatusCodes.Status401Unauthorized,
            Detail = authorizeResult.Forbidden
                ? "You do not have permission to access this resource."
                : "Authentication is required to access this resource."
        };
        context.Response.StatusCode = problemDetails.Status!.Value;
        context.Response.ContentType = ProblemContentType;
        ProblemDetailsContext problemDetailsContext = new()
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        };
        await problemDetailsService.WriteAsync(problemDetailsContext).ConfigureAwait(false);
    }
}