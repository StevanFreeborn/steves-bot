namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record IdentifyDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new IdentifyData Data { get; init; } = new IdentifyData();

  public IdentifyDiscordEvent(string token, long intents, UpdatePresenceData presence)
  {
    OpCode = DiscordOpCodes.Identify;
    Data = new IdentifyData
    {
      Token = token,
      Intents = intents,
      Presence = presence,
    };
  }
}