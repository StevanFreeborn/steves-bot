namespace StevesBot.Worker.Discord.Events;

internal sealed record HelloDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new HelloData? Data { get; init; }
}

internal sealed record HelloData
{
  [JsonPropertyName("heartbeat_interval")]
  public int HeartbeatInterval { get; init; }
}