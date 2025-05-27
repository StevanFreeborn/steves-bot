namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubeVideo
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("liveStreamingDetails")]
  public YouTubeLiveStreamingDetails? LiveStreamingDetails { get; init; }

  public bool IsStream => LiveStreamingDetails is not null;
}