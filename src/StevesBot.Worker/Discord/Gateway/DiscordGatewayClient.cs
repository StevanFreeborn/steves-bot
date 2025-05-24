using Activity = StevesBot.Worker.Discord.Gateway.Events.Data.Activity;

namespace StevesBot.Worker.Discord.Gateway;

internal sealed class DiscordGatewayClient : IDiscordGatewayClient
{
  private const string VersionKey = "v";
  private const string VersionValue = "10";
  private const string EncodingKey = "encoding";
  private const string EncodingValue = "json";

  private readonly DiscordClientOptions _options;
  private readonly IWebSocketFactory _webSocketFactory;
  private readonly ILogger<DiscordGatewayClient> _logger;
  private readonly IDiscordRestClient _discordRestClient;
  private readonly TimeProvider _timeProvider;
  private readonly IServiceScopeFactory _serviceScopeFactory;
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    Converters =
    {
      new DiscordEventConverter(),
    },
  };
  private readonly AsyncLock _lock = new();
  private readonly Dictionary<string, Func<DiscordEvent, IServiceProvider, CancellationToken, Task>> _eventHandlers = [];

  private string _gatewayUrl = string.Empty;
  private IWebSocket? _webSocket;
  private int? _lastSequence;
  private int? _lastDispatchSequence;
  private int _heartbeatInterval;
  private DateTimeOffset _timeLastHeartbeatSent = DateTimeOffset.MinValue;
  private DateTimeOffset _timeLastHeartbeatAcknowledged = DateTimeOffset.MinValue;
  private CancellationTokenSource? _receiveMessageCts;
  private CancellationTokenSource? _linkedReceiveMessageCts;
  private CancellationTokenSource? _heartbeatCts;
  private CancellationTokenSource? _linkedHeartbeatCts;
  private bool _canResume;
  private string _sessionId = string.Empty;
  private string _resumeGatewayUrl = string.Empty;

  public DiscordGatewayClient(
    DiscordClientOptions options,
    IWebSocketFactory webSocketFactory,
    ILogger<DiscordGatewayClient> logger,
    IDiscordRestClient discordRestClient,
    TimeProvider timeProvider,
    IServiceScopeFactory serviceScopeFactory
  )
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _webSocketFactory = webSocketFactory ?? throw new ArgumentNullException(nameof(webSocketFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _discordRestClient = discordRestClient ?? throw new ArgumentNullException(nameof(discordRestClient));
    _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
    _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
  }

  public async Task ConnectAsync(CancellationToken cancellationToken)
  {
    await SetCanResumeAsync(false, cancellationToken);

    if (string.IsNullOrWhiteSpace(_gatewayUrl))
    {
      var gatewayUrl = await _discordRestClient.GetGatewayUrlAsync(cancellationToken);
      await SetGatewayUrlAsync(gatewayUrl, cancellationToken);
    }

    await SetWebSocketAsync(_webSocketFactory.Create(), cancellationToken);
    await ConnectWithGatewayUrlAsync(cancellationToken);

    _logger.LogInformation("Connected to Discord Gateway at {GatewayUrl}", _gatewayUrl);

    await StartReceiveMessagesAsync(cancellationToken);
  }

  public async Task DisconnectAsync(CancellationToken cancellationToken)
  {
    await SendIdleStatusAsync(cancellationToken);
    await CancelReceiveMessagesTaskAsync(cancellationToken);
    await CancelHeartbeatTaskAsync(cancellationToken);
    await CloseIfOpenAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", cancellationToken);
  }

  public void On(string eventName, Func<DiscordEvent, IServiceProvider, CancellationToken, Task> handler)
  {
    if (DiscordEventTypes.IsValidEvent(eventName) is false)
    {
      throw new ArgumentException($"Invalid event name: {eventName}", nameof(eventName));
    }

    ArgumentNullException.ThrowIfNull(handler);

    if (_eventHandlers.ContainsKey(eventName) is false)
    {
      _eventHandlers[eventName] = handler;
      return;
    }

    _eventHandlers[eventName] += handler;
  }

  public void Off(string eventName, Func<DiscordEvent, IServiceProvider, CancellationToken, Task> handler)
  {
    if (DiscordEventTypes.IsValidEvent(eventName) is false)
    {
      throw new ArgumentException($"Invalid event name: {eventName}", nameof(eventName));
    }

    ArgumentNullException.ThrowIfNull(handler);

    if (_eventHandlers.TryGetValue(eventName, out var handlers))
    {
      handlers -= handler;

      if (handlers is null)
      {
        _eventHandlers.Remove(eventName);
      }
      else
      {
        _eventHandlers[eventName] = handlers;
      }
    }
  }

  private async Task StartReceiveMessagesAsync(CancellationToken cancellationToken)
  {
    var newReceiveCts = new CancellationTokenSource();
    var newReceiveLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, newReceiveCts.Token);

    await SetReceiveCtsAsync(newReceiveCts, cancellationToken);
    await SetLinkedReceiveCtsAsync(newReceiveLinkedCts, cancellationToken);

    _ = Task.Run(async () =>
    {
      _logger.LogInformation("Starting receive messages task.");

      try
      {
        var messageBuffer = new byte[8192];

        while (_linkedReceiveMessageCts?.IsCancellationRequested is false)
        {
          using var memoryStream = new MemoryStream();
          WebSocketReceiveResult result;

          do
          {
            result = await ReceiveMessageAsync(new(messageBuffer), _linkedReceiveMessageCts.Token);

            if (result.MessageType is WebSocketMessageType.Close)
            {
              if (DiscordCloseCodes.IsReconnectable((int?)result.CloseStatus) is false)
              {
                _logger.LogCritical("Received close status indicating we should not attempt reconnection: {CloseStatus}", result.CloseStatus);
                await DisconnectAsync(cancellationToken);
                return;
              }

              var canResume = IsResumableCloseCode(result.CloseStatus);

              await SetCanResumeAsync(canResume, _linkedReceiveMessageCts.Token);

              _logger.LogWarning("Reconnecting because of close message: {CloseStatus} - {CloseStatusDescription}", result.CloseStatus, result.CloseStatusDescription);

              await ReconnectAsync(cancellationToken);

              return;
            }

            if (result.MessageType is WebSocketMessageType.Text)
            {
              await memoryStream.WriteAsync(messageBuffer.AsMemory(0, result.Count), _linkedReceiveMessageCts.Token);
              await memoryStream.FlushAsync(_linkedReceiveMessageCts.Token);
            }

          } while (result.EndOfMessage is false);

          memoryStream.Seek(0, SeekOrigin.Begin);

          var msg = Encoding.UTF8.GetString(messageBuffer, 0, result.Count);

          _logger.LogDebug("Received message: {Message}", msg);

          var e = await JsonSerializer.DeserializeAsync<DiscordEvent>(
            memoryStream,
            _jsonSerializerOptions,
            _linkedReceiveMessageCts.Token
          );

          if (e is null)
          {
            _logger.LogInformation("Received null event.");
            continue;
          }

          await HandleEventAsync(e, cancellationToken);
        }
      }
      catch (OperationCanceledException ex)
      {
        _logger.LogInformation(ex, "Receive messages operation canceled");
      }
#pragma warning disable CA1031 // Do not catch general exception types
      catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
      {
        _logger.LogError(ex, "Unexpected error while receiving messages");

        _logger.LogWarning("Closing connection and invalidating session.");

        await SetCanResumeAsync(false, newReceiveLinkedCts.Token);

        _logger.LogWarning("Reconnecting because of error in receive message task");

        await ReconnectAsync(cancellationToken);
      }
    }, newReceiveLinkedCts.Token);
  }

  private async Task HandleEventAsync(DiscordEvent e, CancellationToken cancellationToken)
  {
    await SetSequenceAsync(e.Sequence, cancellationToken);

    switch (e)
    {
      case HelloDiscordEvent he:
        await SetHeartbeatIntervalAsync(he.Data.HeartbeatInterval, cancellationToken);
        await StartHeartbeatAsync(cancellationToken);
        await IdentifyAsync(cancellationToken);
        _logger.LogInformation("Hello event received");
        break;
      case HeartbeatAckDiscordEvent:
        await SetHeartbeatAcknowledgedAsync(_timeProvider.GetUtcNow(), cancellationToken);
        _logger.LogInformation("Heartbeat acknowledged");
        break;
      case HeartbeatDiscordEvent:
        _logger.LogInformation("Heartbeat request received");
        await SendHeartbeatAsync(cancellationToken);
        break;
      case DispatchDiscordEvent de:
        await SetDispatchSequenceAsync(e.Sequence, cancellationToken);
        var eventType = de.Type ?? "Unknown";

        if (de is ReadyDiscordEvent re)
        {
          await SetSessionIdAsync(re.Data.SessionId, cancellationToken);
          await SetResumeGatewayUrlAsync(re.Data.ResumeGatewayUrl, cancellationToken);
          _logger.LogInformation("Ready event received");
        }

        if (_eventHandlers.TryGetValue(eventType, out var handler))
        {
          try
          {
            await using var scope = _serviceScopeFactory.CreateAsyncScope();
            // TODO: Pass cancellation token to handler
            await handler(de, scope.ServiceProvider, cancellationToken);
          }
# pragma warning disable CA1031 // Do not catch general exception types
          catch (Exception ex)
          {
            _logger.LogError(ex, "Error handling event: {Event}", eventType);
          }
# pragma warning restore CA1031 // Do not catch general exception types
        }
        else
        {
          _logger.LogInformation("No handler for event: {Event}", eventType);
        }

        break;
      case ReconnectDiscordEvent:
        _logger.LogInformation("Reconnect event received");

        _logger.LogWarning("Closing connection without invalidating session.");

        await SetCanResumeAsync(true, cancellationToken);

        _logger.LogWarning("Reconnecting because of reconnect event");

        await ReconnectAsync(cancellationToken);
        break;
      case InvalidSessionDiscordEvent ise:
        _logger.LogInformation("Invalid session event received");

        if (ise.Data)
        {
          _logger.LogWarning("Session is resumable. Closing connection without invalidating session.");

          await SetCanResumeAsync(true, cancellationToken);
        }
        else
        {
          _logger.LogWarning("Session is not resumable. Closing connection and invalidating session.");

          await SetCanResumeAsync(false, cancellationToken);
        }

        await ReconnectAsync(cancellationToken);
        break;
      default:
        _logger.LogInformation("Received event: {Event}", e.GetType().Name);
        break;
    }
  }

  private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
  {
    var newHeartbeatCts = new CancellationTokenSource();
    var newLinkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, newHeartbeatCts.Token);

    await SetHeartbeatCtsAsync(newHeartbeatCts, cancellationToken);
    await SetLinkedHeartbeatCtsAsync(newLinkedCts, cancellationToken);

    _ = Task.Run(async () =>
    {
      _logger.LogInformation("Starting heartbeat task.");

      while (_linkedHeartbeatCts?.Token.IsCancellationRequested is false)
      {
        try
        {
          var heartbeatInterval = CalculateHeartbeatInterval();

          await Task.Delay(heartbeatInterval, _linkedHeartbeatCts.Token);

          if (IsHeartbeatAcknowledged() is false)
          {
            if (IsWebSocketOpen())
            {
              await SetCanResumeAsync(true, _linkedHeartbeatCts.Token);
            }

            _logger.LogWarning("Heartbeat not acknowledged. Reconnecting.");

            await ReconnectAsync(cancellationToken);

            break;
          }

          await SendHeartbeatAsync(_linkedHeartbeatCts.Token);

          _logger.LogInformation("Heartbeat sent");
        }
        catch (OperationCanceledException ex)
        {
          _logger.LogInformation(ex, "Heartbeat task canceled");
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception ex)
#pragma warning restore CA1031 // Do not catch general exception types
        {
          _logger.LogError(ex, "Error in heartbeat task: {Message}", ex.Message);

          _logger.LogWarning("Reconnecting because of error in heartbeat task");

          await ReconnectAsync(cancellationToken);
        }
      }
    }, newLinkedCts.Token);
  }

  private async Task ReconnectAsync(CancellationToken cancellationToken)
  {
    await CancelHeartbeatTaskAsync(cancellationToken);
    await CancelReceiveMessagesTaskAsync(cancellationToken);

    var closeStatus = _canResume ? WebSocketCloseStatus.MandatoryExtension : WebSocketCloseStatus.NormalClosure;

    await CloseIfOpenAsync(
      closeStatus,
      "Reconnecting",
      cancellationToken
    );

    _webSocket?.Dispose();

    if (_canResume)
    {
      _logger.LogInformation("Resuming connection to Discord Gateway.");

      await SetWebSocketAsync(_webSocketFactory.Create(), cancellationToken);
      await ConnectWithResumeUrlAsync(cancellationToken);
      await SendResumeAsync(cancellationToken);
      await StartReceiveMessagesAsync(cancellationToken);
      await StartHeartbeatAsync(cancellationToken);
      return;
    }

    _logger.LogInformation("Reconnecting to Discord Gateway.");
    await ConnectAsync(cancellationToken);
  }

  private async Task SendResumeAsync(CancellationToken cancellationToken)
  {
    ResumeDiscordEvent resume;

    using (await _lock.LockAsync(cancellationToken))
    {
      if (_lastDispatchSequence is null)
      {
        throw new DiscordGatewayClientException("Cannot resume without a sequence number.");
      }

      resume = new ResumeDiscordEvent(
        _options.AppToken,
        _sessionId,
        _lastDispatchSequence.Value
      );
    }

    await SendJsonAsync(resume, cancellationToken);
  }

  private async Task IdentifyAsync(CancellationToken cancellationToken)
  {
    var identify = new IdentifyDiscordEvent(
      _options.AppToken,
      _options.Intents,
      new UpdatePresenceData
      {
        Status = PresenceStatus.Online,
        Activities = [
          new()
          {
            Name = "Helping Stevan",
            State = "Helping Stevan"
          }
        ],
      }
    );

    await SendJsonAsync(identify, cancellationToken);
  }

  private async Task SendIdleStatusAsync(CancellationToken cancellationToken)
  {
    var presence = new UpdatePresenceDiscordEvent(
      _timeProvider.GetUtcNow().Millisecond,
      [new Activity
      {
        Name = "Taking a break. Stevan's got this.",
        Type = ActivityType.Custom,
        State = "Taking a break. Stevan's got this.",
      }],
      PresenceStatus.Idle,
      true
    );

    await SendJsonAsync(presence, cancellationToken);
  }

  private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
  {
    var heartbeat = new HeartbeatDiscordEvent(_lastSequence);
    await SendJsonAsync(heartbeat, cancellationToken);
    await SetHeartbeatSentAsync(_timeProvider.GetUtcNow(), cancellationToken);
  }

  private async Task SendJsonAsync(object data, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);

    if (_webSocket is null)
    {
      throw new DiscordGatewayClientException("WebSocket is not set. Cannot send heartbeat.");
    }

    if (_webSocket.State is not WebSocketState.Open)
    {
      throw new DiscordGatewayClientException("WebSocket is not open. Cannot send heartbeat.");
    }

    try
    {
      var json = JsonSerializer.Serialize(data, _jsonSerializerOptions);
      var bytes = Encoding.UTF8.GetBytes(json);
      var buffer = new ArraySegment<byte>(bytes);
      await _webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationToken);
    }
    catch (OperationCanceledException ex)
    {
      _logger.LogInformation(ex, "Send operation canceled");
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Failed to send message: {Message}", ex.Message);
      throw new DiscordGatewayClientException("Failed to send message.", ex);
    }
  }

  private bool IsResumableCloseCode(WebSocketCloseStatus? closeStatus)
  {
    if (string.IsNullOrWhiteSpace(_resumeGatewayUrl) || string.IsNullOrWhiteSpace(_sessionId))
    {
      return false;
    }

    if (closeStatus is WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable)
    {
      return false;
    }

    return true;
  }

  private bool IsWebSocketOpen()
  {
    return _webSocket?.State is WebSocketState.Open;
  }

  private bool IsHeartbeatAcknowledged()
  {
    return _timeLastHeartbeatAcknowledged >= _timeLastHeartbeatSent;
  }

  private int CalculateHeartbeatInterval()
  {
#pragma warning disable CA5394 // Do not use insecure randomness
    var jitter = Random.Shared.NextDouble();
#pragma warning restore CA5394 // Do not use insecure randomness
    return (int)(jitter * _heartbeatInterval);
  }

  private async Task CancelReceiveMessagesTaskAsync(CancellationToken cancellationToken)
  {
    if (_receiveMessageCts is not null)
    {
      await _receiveMessageCts.CancelAsync();
      _receiveMessageCts.Dispose();
      await SetReceiveCtsAsync(null, cancellationToken);
    }

    if (_linkedReceiveMessageCts is not null)
    {
      await _linkedReceiveMessageCts.CancelAsync();
      _linkedReceiveMessageCts.Dispose();
      await SetLinkedReceiveCtsAsync(null, cancellationToken);
    }
  }

  private async Task SetLinkedReceiveCtsAsync(CancellationTokenSource? cts, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _linkedReceiveMessageCts = cts;
  }

  private async Task SetReceiveCtsAsync(CancellationTokenSource? cts, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _receiveMessageCts = cts;
  }


  private async Task CancelHeartbeatTaskAsync(CancellationToken cancellationToken)
  {
    if (_heartbeatCts is not null)
    {
      await _heartbeatCts.CancelAsync();
      _heartbeatCts.Dispose();
      await SetHeartbeatCtsAsync(null, cancellationToken);
    }

    if (_linkedHeartbeatCts is not null)
    {
      await _linkedHeartbeatCts.CancelAsync();
      _linkedHeartbeatCts.Dispose();
      await SetLinkedHeartbeatCtsAsync(null, cancellationToken);
    }
  }

  private async Task SetLinkedHeartbeatCtsAsync(CancellationTokenSource? cts, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _linkedHeartbeatCts = cts;
  }

  private async Task SetHeartbeatCtsAsync(CancellationTokenSource? cts, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _heartbeatCts = cts;
  }


  private async Task CloseIfOpenAsync(
    WebSocketCloseStatus closeStatus,
    string? statusDescription,
    CancellationToken cancellationToken
  )
  {
    if (_webSocket?.State is not WebSocketState.Open)
    {
      return;
    }

    await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
  }

  private async Task<WebSocketReceiveResult> ReceiveMessageAsync(
    ArraySegment<byte> segment,
    CancellationToken cancellationToken
  )
  {
    if (_webSocket is null)
    {
      throw new DiscordGatewayClientException("WebSocket is not set. Cannot receive message.");
    }

    if (_webSocket.State is not WebSocketState.Open)
    {
      throw new DiscordGatewayClientException("WebSocket is not open. Cannot receive message.");
    }

    var result = await _webSocket.ReceiveAsync(segment, cancellationToken);
    return result;
  }

  private Task ConnectWithGatewayUrlAsync(CancellationToken cancellationToken)
  {
    var uri = BuildUri(_gatewayUrl);
    return ConnectWithUriAsync(uri, cancellationToken);
  }

  private Task ConnectWithResumeUrlAsync(CancellationToken cancellationToken)
  {
    var uri = BuildUri(_resumeGatewayUrl);
    return ConnectWithUriAsync(uri, cancellationToken);
  }

  private async Task ConnectWithUriAsync(Uri uri, CancellationToken cancellationToken)
  {
    if (_webSocket is null)
    {
      throw new DiscordGatewayClientException("WebSocket is not set. Cannot connect.");
    }

    if (_webSocket.State is WebSocketState.Open)
    {
      throw new DiscordGatewayClientException("WebSocket is already open. Cannot connect.");
    }

    await _webSocket.ConnectAsync(uri, cancellationToken);
  }

  private static Uri BuildUri(string url)
  {
    return new UriBuilder(url)
    {
      Query = $"{VersionKey}={VersionValue}&{EncodingKey}={EncodingValue}"
    }.Uri;
  }

  private async Task SetGatewayUrlAsync(string url, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _gatewayUrl = url;
  }

  private async Task SetDispatchSequenceAsync(int? sequence, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _lastDispatchSequence = sequence;
  }

  private async Task SetSequenceAsync(int? sequence, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _lastSequence = sequence;
  }

  private async Task SetHeartbeatIntervalAsync(int interval, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _heartbeatInterval = interval;
  }

  private async Task SetHeartbeatSentAsync(DateTimeOffset time, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _timeLastHeartbeatSent = time;
  }

  private async Task SetHeartbeatAcknowledgedAsync(DateTimeOffset time, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _timeLastHeartbeatAcknowledged = time;
  }

  private async Task SetSessionIdAsync(string sessionId, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _sessionId = sessionId;
  }

  private async Task SetResumeGatewayUrlAsync(string url, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _resumeGatewayUrl = url;
  }

  private async Task SetWebSocketAsync(IWebSocket webSocket, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _webSocket = webSocket;
  }

  private async Task SetCanResumeAsync(bool canResume, CancellationToken cancellationToken)
  {
    using var _ = await _lock.LockAsync(cancellationToken);
    _canResume = canResume;
  }

  public void Dispose()
  {
    _heartbeatCts?.Dispose();
    _linkedHeartbeatCts?.Dispose();

    _receiveMessageCts?.Dispose();
    _linkedReceiveMessageCts?.Dispose();

    _webSocket?.Dispose();
    _lock.Dispose();
  }
}