using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using WebApi.Common.Logging;
using static WebApi.Common.Constants;

namespace WebApi.Common.Web;

public static class ErrorHandlingExtensions
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
        context.Response.ContentType = "application/json";
        var problem = new
        {
            title = "An unexpected error occurred",
            status = StatusCodes.Status500InternalServerError,
            traceId = context.TraceIdentifier
        };
        await context.Response.WriteAsJsonAsync(problem, cancellationToken).ConfigureAwait(false);
    }
}