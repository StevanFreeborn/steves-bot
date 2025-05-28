using System.Text.Json.Serialization;

namespace StevesBot.Library.Discord.Common;

public sealed record DiscordMessageReference(
  [property: JsonPropertyName("type")] int Type,
  [property: JsonPropertyName("message_id")] string MessageId,
  [property: JsonPropertyName("channel_id")] string ChannelId,
  [property: JsonPropertyName("guild_id")] string GuildId,
  [property: JsonPropertyName("fail_if_not_exists")] bool FailIfNotExists
);