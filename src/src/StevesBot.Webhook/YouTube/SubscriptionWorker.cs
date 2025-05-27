namespace StevesBot.Webhook.YouTube;

internal sealed class SubscriptionWorker(
  ILogger<SubscriptionWorker> logger,
  IOptions<SubscriptionOptions> options,
  IPubSubClient pubSubClient
) : IHostedLifecycleService
{
  private readonly ILogger<SubscriptionWorker> _logger = logger;
  private readonly SubscriptionOptions _options = options.Value;
  private readonly IPubSubClient _pubSubClient = pubSubClient;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public Task StartingAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public async Task StartedAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Subscribing to notifications");

    var isSubscribed = await _pubSubClient.SubscribeAsync(
      _options.CallbackUrl,
      _options.TopicUrl,
      cancellationToken
    );

    if (isSubscribed)
    {
      _logger.LogInformation("Successfully subscribed to notifications");
      return;
    }

    _logger.LogWarning("Failed to subscribe to notifications");
  }


  public Task StopAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public Task StoppingAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }

  public Task StoppedAsync(CancellationToken cancellationToken)
  {
    return Task.CompletedTask;
  }
}