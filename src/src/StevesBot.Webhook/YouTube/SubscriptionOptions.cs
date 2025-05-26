namespace StevesBot.Webhook.YouTube;

internal sealed record SubscriptionOptions
{
  [Required]
  public string CallbackBaseUrl { get; init; } = string.Empty;

  [Required]
  public string TopicUrl { get; init; } = string.Empty;
}