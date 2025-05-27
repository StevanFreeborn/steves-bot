using System.Text.Json.Serialization;

namespace StevesBot.Webhook.YouTube.Data;

internal sealed record YouTubePageInfo
{
  [JsonPropertyName("totalResults")]
  public int TotalResults { get; init; }

  [JsonPropertyName("resultsPerPage")]
  public int ResultsPerPage { get; init; }
}