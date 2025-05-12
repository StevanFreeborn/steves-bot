namespace StevesBot.Worker.Discord;

internal sealed class DiscordGatewayClientOptions
{
  public string ApiUrl { get; init; } = string.Empty;
  public string AppToken { get; init; } = string.Empty;
  public long Intents { get; init; }
}