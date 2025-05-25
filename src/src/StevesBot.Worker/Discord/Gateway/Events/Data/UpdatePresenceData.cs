namespace StevesBot.Worker.Discord.Gateway.Events.Data;

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