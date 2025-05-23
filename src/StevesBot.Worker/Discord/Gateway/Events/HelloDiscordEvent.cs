namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record HelloDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new HelloData Data { get; init; } = new HelloData();

  public HelloDiscordEvent()
  {
    OpCode = DiscordOpCodes.Hello;
  }
}
