using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Common;

public sealed record DiscordMessage
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;

  [JsonPropertyName("type")]
  public int Type { get; init; }

  [JsonPropertyName("channel_id")]
  public string ChannelId { get; init; } = string.Empty;

  [JsonPropertyName("guild_id")]
  public string GuildId { get; init; } = string.Empty;

  [JsonPropertyName("author")]
  public DiscordUser Author { get; init; } = new DiscordUser();

  [JsonPropertyName("content")]
  public string Content { get; init; } = string.Empty;

  [JsonPropertyName("mentions")]
  public IEnumerable<DiscordUser> Mentions { get; init; } = [];

  public bool MentionsUser(string userId)
  {
    return Mentions.Any(u => u.Id.Equals(userId, StringComparison.OrdinalIgnoreCase));
  }
}