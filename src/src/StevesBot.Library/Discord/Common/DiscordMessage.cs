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

  public string GetThreadNameFromContent()
  {
    var content = Content;

    foreach (var mention in Mentions)
    {
      var mentionText = $"<@{mention.Id}>";

      content = content.Replace(
        mentionText,
        string.Empty,
        StringComparison.OrdinalIgnoreCase
      );
    }

    if (string.IsNullOrWhiteSpace(content))
    {
      return $"Thread for message {Id}";
    }

    const int maxNameLength = 100;
    var nameEndIndex = Math.Min(content.Length, maxNameLength);

    return content[..nameEndIndex];
  }
}