namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubeLiveStreamingDetails
{
  [JsonPropertyName("actualStartTime")]
  public DateTimeOffset? ActualStartTime { get; init; }

  [JsonPropertyName("actualEndTime")]
  public DateTimeOffset? ActualEndTime { get; init; }

  [JsonPropertyName("scheduledStartTime")]
  public DateTimeOffset? ScheduledStartTime { get; init; }

  [JsonPropertyName("scheduledEndTime")]
  public DateTimeOffset? ScheduledEndTime { get; init; }

  [JsonPropertyName("concurrentViewers")]
  public ulong? ConcurrentViewers { get; init; }

  [JsonPropertyName("activeLiveChatId")]
  public string? ActiveLiveChatId { get; init; }
}