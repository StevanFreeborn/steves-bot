namespace StevesBot.Worker.Discord.Events;

internal sealed record UpdatePresenceDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new UpdatePresenceData Data { get; init; } = new UpdatePresenceData();

  public UpdatePresenceDiscordEvent(long? since, List<Activity> activities, string status, bool afk)
  {
    OpCode = DiscordOpCodes.PresenceUpdate;
    Data = new UpdatePresenceData
    {
      Since = since,
      Activities = activities,
      Status = status,
      Afk = afk,
    };
  }
}

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