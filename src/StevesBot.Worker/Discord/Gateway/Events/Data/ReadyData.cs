namespace StevesBot.Worker.Discord.Gateway.Events.Data;

internal sealed record ReadyData
{
  [JsonPropertyName("v")]
  public int Version { get; init; }

  [JsonPropertyName("session_id")]
  public string SessionId { get; init; } = string.Empty;

  [JsonPropertyName("resume_gateway_url")]
  public string ResumeGatewayUrl { get; init; } = string.Empty;
}