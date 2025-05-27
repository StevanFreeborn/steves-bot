namespace StevesBot.Webhook.YouTube.Tasks;

internal abstract record SubscriptionTask
{
  public string CallbackUrl { get; init; } = string.Empty;
  public string TopicUrl { get; init; } = string.Empty;
}
