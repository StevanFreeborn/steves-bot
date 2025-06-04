namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubeVideo
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("liveStreamingDetails")]
  public YouTubeLiveStreamingDetails? LiveStreamingDetails { get; init; }

  [JsonPropertyName("snippet")]
  public YouTubeSnippet Snippet { get; init; } = new();

  public bool IsLiveStream => LiveStreamingDetails is not null && Snippet.LiveBroadcastContent is "live";
}