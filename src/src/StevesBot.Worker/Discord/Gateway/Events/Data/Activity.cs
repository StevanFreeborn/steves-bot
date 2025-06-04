namespace StevesBot.Worker.Discord.Gateway.Events.Data;

internal sealed record Activity
{
  [JsonPropertyName("name")]
  public string Name { get; init; } = string.Empty;

  [JsonPropertyName("type")]
  public int Type { get; init; } = ActivityType.Custom;

  [JsonPropertyName("state")]
  public string? State { get; init; }
}