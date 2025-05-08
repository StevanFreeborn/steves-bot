namespace StevesBot.Worker.Discord.Events;

internal record DiscordEvent
{
  [JsonPropertyName("op")]
  public int OpCode { get; init; }

  [JsonPropertyName("s")]
  public int? Sequence { get; init; }

  [JsonPropertyName("t")]
  public string? Type { get; init; }

  [JsonPropertyName("d")]
  public object? Data { get; init; }
}