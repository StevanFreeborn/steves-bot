namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubeVideoListResponse
{
  [JsonPropertyName("items")]
  public YouTubeVideo[] Items { get; init; } = [];

  [JsonPropertyName("pageInfo")]
  public YouTubePageInfo PageInfo { get; init; } = new YouTubePageInfo();
}