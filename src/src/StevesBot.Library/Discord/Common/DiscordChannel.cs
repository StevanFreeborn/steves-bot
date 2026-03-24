using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Common;

public sealed record DiscordChannel
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("type")]
  public int Type { get; init; }

  public bool IsChannelType(int type)
  {
    return Type == type;
  }
}