namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record ResumeDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new ResumeData Data { get; init; } = new();

  public ResumeDiscordEvent(
    string token,
    string sessionId,
    int sequence
  )
  {
    OpCode = DiscordOpCodes.Resume;
    Data = new ResumeData
    {
      Token = token,
      SessionId = sessionId,
      Sequence = sequence
    };
  }
}