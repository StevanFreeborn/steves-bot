namespace StevesBot.Worker.Discord;

internal class DiscordClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string AppToken { get; init; } = string.Empty;
  public int Intents { get; init; }
}
