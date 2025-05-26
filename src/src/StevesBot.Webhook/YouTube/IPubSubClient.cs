namespace StevesBot.Webhook.YouTube;

internal interface IPubSubClient
{
  Task SubscribeAsync(
    string callbackUrl,
    string topicUrl,
    CancellationToken cancellationToken = default
  );

  Task UnsubscribeAsync(
    string callbackUrl,
    string topicUrl,
    CancellationToken cancellationToken = default
  );
}