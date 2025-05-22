namespace StevesBot.Worker.Discord.Shared;

internal sealed record DiscordMessageReference(
  [property: JsonPropertyName("type")] int Type,
  [property: JsonPropertyName("message_id")] string MessageId,
  [property: JsonPropertyName("channel_id")] string ChannelId,
  [property: JsonPropertyName("guild_id")] string GuildId,
  [property: JsonPropertyName("fail_if_not_exists")] bool FailIfNotExists
);
