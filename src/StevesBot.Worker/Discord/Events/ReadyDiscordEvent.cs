namespace StevesBot.Worker.Discord.Events;

internal record ReadyDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new ReadyData Data { get; init; } = new ReadyData();
}

internal record ReadyData
{
  [JsonPropertyName("v")]
  public int Version { get; init; }

  [JsonPropertyName("session_id")]
  public string SessionId { get; init; } = string.Empty;

  [JsonPropertyName("resume_gateway_url")]
  public string ResumeGatewayUrl { get; init; } = string.Empty;
}
