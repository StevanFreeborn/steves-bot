namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record MessageCreateDiscordEvent : DispatchDiscordEvent
{
  [JsonPropertyName("d")]
  public new DiscordMessage Data { get; init; } = new DiscordMessage();

  public MessageCreateDiscordEvent()
  {
    Type = DiscordEventTypes.MessageCreate;
  }

  public bool IsMessageType(int messageType)
  {
    return Data.Type == messageType;
  }
}