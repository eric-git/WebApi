using System.Text.Json.Serialization;

namespace WebApi.Common;

public class TokenResponse
{
    [JsonIgnore]
    public string? ServiceId { get; set; }

    public required string TokenType { get; set; }

    public required string AccessToken { get; set; }

    public required int ExpiresIn { get; set; }

    [JsonIgnore]
    public DateTime ExpiryUtc { get; set; }
}