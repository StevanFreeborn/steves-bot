namespace StevesBot.Webhook.YouTube.Tasks;

internal sealed record SubscribeTask : SubscriptionTask
{
  public DateTimeOffset ExpiresAt { get; init; }
}
