namespace StevesBot.Worker.Discord.Events;

internal record HelloDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new HelloData? Data { get; init; }
}

internal record HelloData
{
  [JsonPropertyName("heartbeat_interval")]
  public int HeartbeatInterval { get; init; }
}
