using System.Text.Json.Serialization;

namespace Jarvis.Models;

public sealed class ChatSession
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    [JsonPropertyName("userId")]
    public string UserId { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = "New chat";

    [JsonPropertyName("createdAtUtc")]
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("updatedAtUtc")]
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    [JsonPropertyName("messages")]
    public List<ChatMessage> Messages { get; set; } = [];
}

public sealed record ChatSessionSummary(
    string Id,
    string Title,
    string Preview,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc,
    int MessageCount);
