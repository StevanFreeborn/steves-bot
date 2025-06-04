namespace StevesBot.Webhook.YouTube;

internal sealed record DiscordNotificationOptions
{
  [Required]
  public string ChannelId { get; init; } = string.Empty;

  [Required]
  public string MessageFormat { get; init; } = string.Empty;
}