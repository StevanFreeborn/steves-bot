namespace StevesBot.Worker.Discord.Events;

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

internal sealed record IdentifyData
{
  [JsonPropertyName("token")]
  public string Token { get; init; } = string.Empty;

  [JsonPropertyName("properties")]
  public IdentifyProperties Properties { get; init; } = new();

  [JsonPropertyName("presence")]
  public UpdatePresenceData Presence { get; init; } = new();

  [JsonPropertyName("intents")]
  public long Intents { get; init; }
}

internal sealed record IdentifyProperties
{
  [JsonPropertyName("os")]
  public string Os { get; init; } = Environment.OSVersion.ToString();

  [JsonPropertyName("browser")]
  public string Browser { get; init; } = Assembly.GetExecutingAssembly().GetName().FullName;

  [JsonPropertyName("device")]
  public string Device { get; init; } = Assembly.GetExecutingAssembly().GetName().FullName;
}
