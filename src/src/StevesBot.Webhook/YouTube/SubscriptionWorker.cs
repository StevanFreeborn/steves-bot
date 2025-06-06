namespace StevesBot.Webhook.YouTube;

internal sealed class SubscriptionWorker(
  ILogger<SubscriptionWorker> logger,
  IOptions<SubscriptionOptions> options,
  IPubSubClient pubSubClient,
  ConcurrentQueue<SubscribeTask> subscriptionQueue,
  TimeProvider timeProvider
) : IHostedLifecycleService, IDisposable
{
  private readonly ILogger<SubscriptionWorker> _logger = logger;
  private readonly SubscriptionOptions _options = options.Value;
  private readonly IPubSubClient _pubSubClient = pubSubClient;
  private readonly ConcurrentQueue<SubscribeTask> _subscriptionQueue = subscriptionQueue;
  private readonly TimeProvider _timeProvider = timeProvider;
  private ITimer? _subscriptionTimer;

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Starting subscription worker");

    _subscriptionTimer = _timeProvider.CreateTimer(
      callback: async _ => await ProcessSubscriptionQueueAsync(cancellationToken),
      state: null,
      dueTime: TimeSpan.FromSeconds(0),
      period: TimeSpan.FromDays(1)
    );

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
    _subscriptionTimer?.Change(Timeout.InfiniteTimeSpan, TimeSpan.Zero);
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

  private async Task ProcessSubscriptionQueueAsync(CancellationToken cancellationToken)
  {
    var tasksToRequeue = new List<SubscribeTask>();

    while (_subscriptionQueue.TryDequeue(out var task))
    {
      // We will attempt to process the task
      // once every day so we will aim to 
      // process it two days before it expires.
      var now = _timeProvider.GetUtcNow();
      var dueAt = task.ExpiresAt - TimeSpan.FromDays(2);
      var isDue = now >= dueAt;

      if (isDue is false)
      {
        _logger.LogInformation(
          "Skipping subscription task for topic {TopicUrl} as it is not due yet. Due at: {DueAt}, Current time: {CurrentTime}",
          task.TopicUrl,
          dueAt,
          now
        );

        tasksToRequeue.Add(task);

        continue;
      }

      _logger.LogInformation("Processing subscription task for topic: {TopicUrl}", task.TopicUrl);

      var isSubscribed = await _pubSubClient.SubscribeAsync(
        task.CallbackUrl,
        task.TopicUrl,
        cancellationToken
      );

      if (isSubscribed)
      {
        _logger.LogInformation("Successfully subscribed to topic: {TopicUrl}", task.TopicUrl);
        return;
      }

      _logger.LogWarning("Failed to subscribe to topic: {TopicUrl}", task.TopicUrl);
    }

    tasksToRequeue.ForEach(_subscriptionQueue.Enqueue);
  }

  public void Dispose()
  {
    _subscriptionTimer?.Dispose();
  }
}