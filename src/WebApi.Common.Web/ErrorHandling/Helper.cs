using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Common.Logging;
using static WebApi.Common.Constants;
using static WebApi.Common.Logging.Constants;
using static WebApi.Common.Web.ErrorHandling.Constants;

namespace WebApi.Common.Web.ErrorHandling;

public static class Helper
{
    public static async Task HandleExceptionAsync(HttpContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var cancellationToken = context.RequestAborted;
        cancellationToken.ThrowIfCancellationRequested();
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;

        var logger = context.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(GlobalExceptionHandler);
        LogCallback logCallback = logger.Log;
        logCallback(LogLevel.Error, new EventId(ErrorOccurred, nameof(ErrorOccurred)), exception?.Message);

        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        context.Response.ContentType = "application/problem+json";
        ProblemDetails problemDetails = new()
        {
            Status = StatusCodes.Status500InternalServerError,
            Detail = exception?.Message
        };
        ProblemDetailsContext problemDetailsContext = new()
        {
            HttpContext = context,
            ProblemDetails = problemDetails
        };
        var problemDetailsService = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problemDetailsService.WriteAsync(problemDetailsContext).ConfigureAwait(false);
    }

    public static void ConfigureProblemDetails(ProblemDetailsContext problemDetailsContext)
    {
        ArgumentNullException.ThrowIfNull(problemDetailsContext);
        var problemDetails = problemDetailsContext.ProblemDetails;
        var httpContext = problemDetailsContext.HttpContext;
        problemDetails.Extensions[TraceIdPropertyName] = httpContext.TraceIdentifier;
        if (problemDetails.Status is null)
        {
            return;
        }

        problemDetails.Title = ReasonPhrases.GetReasonPhrase(problemDetails.Status.Value);
        problemDetails.Type = $"{BaseErrorTypeUri}/{problemDetails.Status}";
    }
}