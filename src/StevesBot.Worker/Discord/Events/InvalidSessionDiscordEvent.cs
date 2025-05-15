namespace StevesBot.Worker.Discord.Events;

internal sealed record InvalidSessionDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new bool Data { get; init; }
}