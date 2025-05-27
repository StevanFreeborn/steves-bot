namespace StevesBot.Webhook.YouTube.Tasks;

internal sealed record SubscribeTask
{
  public string CallbackUrl { get; init; } = string.Empty;
  public string TopicUrl { get; init; } = string.Empty;
  public DateTimeOffset ExpiresAt { get; init; }
}