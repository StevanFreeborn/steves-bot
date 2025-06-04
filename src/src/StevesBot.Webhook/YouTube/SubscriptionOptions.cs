namespace StevesBot.Webhook.YouTube;

internal sealed record SubscriptionOptions
{
  [Required]
  public string CallbackUrl { get; init; } = string.Empty;

  [Required]
  public string TopicUrl { get; init; } = string.Empty;
}