namespace StevesBot.Worker.Discord.Shared;

internal sealed class DiscordClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string AppToken { get; init; } = string.Empty;
  public long Intents { get; init; }
}