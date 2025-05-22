namespace StevesBot.Worker.Discord.Gateway.Events.Data;

internal sealed record ResumeData
{
  [JsonPropertyName("token")]
  public string Token { get; init; } = string.Empty;

  [JsonPropertyName("session_id")]
  public string SessionId { get; init; } = string.Empty;

  [JsonPropertyName("seq")]
  public int Sequence { get; init; }
}