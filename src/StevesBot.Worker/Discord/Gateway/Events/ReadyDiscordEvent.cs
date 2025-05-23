namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record ReadyDiscordEvent : DispatchDiscordEvent
{
  [JsonPropertyName("t")]
  public new string Type { get; init; } = DiscordEventTypes.Ready;

  [JsonPropertyName("d")]
  public new ReadyData Data { get; init; } = new ReadyData();
}
