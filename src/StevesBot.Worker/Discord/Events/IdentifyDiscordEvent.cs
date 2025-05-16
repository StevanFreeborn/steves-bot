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

// TODO: Move this to an update presence event file

internal sealed record UpdatePresenceData
{
  [JsonPropertyName("since")]
  public long? Since { get; init; } = null;

  [JsonPropertyName("activities")]
  public List<Activity> Activities { get; init; } = [];

  [JsonPropertyName("status")]
  public string Status { get; init; } = PresenceStatus.Online;

  [JsonPropertyName("afk")]
  public bool Afk { get; init; }
}

internal static class PresenceStatus
{
  public const string Online = "online";
}

internal sealed record Activity
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("type")]
  public int Type { get; init; } = ActivityType.Custom;

  [JsonPropertyName("state")]
  public string? State { get; init; }
}

internal static class ActivityType
{
  public const int Custom = 4;
}