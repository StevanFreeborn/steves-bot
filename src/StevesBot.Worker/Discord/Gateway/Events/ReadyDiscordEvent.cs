namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record ReadyDiscordEvent : DispatchDiscordEvent
{

  [JsonPropertyName("d")]
  public new ReadyData Data { get; init; } = new ReadyData();

  public ReadyDiscordEvent()
  {
    Type = DiscordEventTypes.Ready;
  }
}

