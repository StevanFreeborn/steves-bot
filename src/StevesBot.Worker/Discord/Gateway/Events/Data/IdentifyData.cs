namespace StevesBot.Worker.Discord.Gateway.Events.Data;

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
