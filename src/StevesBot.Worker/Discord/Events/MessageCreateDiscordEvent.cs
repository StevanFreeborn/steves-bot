namespace StevesBot.Worker.Discord.Events;

internal sealed record MessageCreateDiscordEvent : DispatchDiscordEvent
{
  [JsonPropertyName("d")]
  public new MessageCreateData Data { get; init; } = new MessageCreateData();

  public bool IsMessageType(int messageType)
  {
    return Data.Type == messageType;
  }
}

internal sealed record MessageCreateData
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

}

internal sealed record DiscordUser
{
  [JsonPropertyName("id")]
  public string Id { get; init; } = string.Empty;
}
