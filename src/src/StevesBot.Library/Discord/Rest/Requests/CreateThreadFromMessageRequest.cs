using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Rest.Requests;

public sealed record CreateThreadFromMessageRequest(
  [property: JsonPropertyName("name")]
  string Name,
  [property: JsonPropertyName("auto_archive_duration")]
  int AutoArchiveDuration = 10080,
  [property: JsonPropertyName("rate_limit_per_user")]
  int RateLimitPerUser = 0
);