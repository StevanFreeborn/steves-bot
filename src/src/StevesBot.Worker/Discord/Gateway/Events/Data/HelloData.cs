namespace StevesBot.Worker.Discord.Gateway.Events.Data;

internal sealed record HelloData
{
  [JsonPropertyName("heartbeat_interval")]
  public int HeartbeatInterval { get; init; }
}