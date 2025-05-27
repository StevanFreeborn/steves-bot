namespace StevesBot.Webhook.YouTube;

internal interface IPubSubClient
{
  Task<bool> SubscribeAsync(
    string callbackUrl,
    string topicUrl,
    CancellationToken cancellationToken = default
  );
}