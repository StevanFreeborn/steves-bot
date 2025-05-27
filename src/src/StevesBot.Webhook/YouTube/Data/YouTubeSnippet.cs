namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubeSnippet
{
  [JsonPropertyName("liveBroadcastContent")]
  public string LiveBroadcastContent { get; init; } = string.Empty;
}