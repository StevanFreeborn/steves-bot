using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Common;

public sealed record DiscordUser
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;
}