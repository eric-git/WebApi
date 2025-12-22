using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using static WebApi.Common.Constants;

namespace WebApi.Common;

public static class LoggingExtensions
{
    private const string Empty = "<empty>";

    private static bool TryFormatFormBody(string content, out string? formatted)
    {
        formatted = null;
        try
        {
            var pairs = content.Split('&', StringSplitOptions.RemoveEmptyEntries);
            formatted = string.Join($"&{Environment.NewLine}", pairs);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static bool TryFormatJsonBody(string content, out string? formatted)
    {
        formatted = null;
        try
        {
            using var jsonDocument = JsonDocument.Parse(content);
            formatted = JsonSerializer.Serialize(jsonDocument.RootElement, DataSerializationOptions);
            return true;
        }
        catch
        {
            return false;
        }
    }

    extension(HttpContent? httpContent)
    {
        private async Task<string> FormatAsync()
        {
            if (httpContent is null)
            {
                return Empty;
            }

            var content = await httpContent.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(content))
            {
                return Empty;
            }

            return httpContent.Headers.ContentType?.MediaType switch
            {
                "application/json" when TryFormatJsonBody(content, out var formattedJson) => formattedJson!,
                "application/x-www-form-urlencoded" when TryFormatFormBody(content, out var formattedForm) => formattedForm!,
                _ => content
            };
        }
    }

    extension(HttpHeaders httpHeaders)
    {
        private string Format()
        {
            var content = httpHeaders.ToString();
            return string.IsNullOrWhiteSpace(content) ? Empty : content;
        }
    }

    extension(HttpRequestMessage httpRequestMessage)
    {
        public async Task LogAsync(ILogger logger)
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            var content = await httpRequestMessage.Content.FormatAsync();
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("=== Request ===");
            stringBuilder.AppendLine($"{httpRequestMessage.Method} {httpRequestMessage.RequestUri}");
            stringBuilder.AppendLine("Headers:");
            stringBuilder.AppendLine(httpRequestMessage.Headers.Format());
            stringBuilder.AppendLine("Body:");
            stringBuilder.AppendLine(content);
            logger.LogInformation(stringBuilder.ToString());
        }
    }

    extension(HttpResponseMessage httpResponseMessage)
    {
        public async Task LogAsync(ILogger logger)
        {
            if (!logger.IsEnabled(LogLevel.Information))
            {
                return;
            }

            var content = await httpResponseMessage.Content.FormatAsync();
            StringBuilder stringBuilder = new();
            stringBuilder.AppendLine("=== Response ===");
            stringBuilder.AppendLine($"Status: {httpResponseMessage.StatusCode}");
            stringBuilder.AppendLine("Headers:");
            stringBuilder.AppendLine(httpResponseMessage.Headers.ToString());
            stringBuilder.AppendLine("Body:");
            stringBuilder.AppendLine(content);
            logger.LogInformation(stringBuilder.ToString());
        }
    }
}