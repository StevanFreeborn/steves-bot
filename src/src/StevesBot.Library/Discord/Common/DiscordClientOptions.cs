namespace StevesBot.Library.Discord.Common;

public sealed class DiscordClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string AppToken { get; init; } = string.Empty;
  public long Intents { get; init; }
}