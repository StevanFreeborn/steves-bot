using System.Text;

namespace StevesBot.Worker.Discord;

internal sealed class DiscordGatewayClient : IDiscordGatewayClient
{
  private readonly DiscordClientOptions _options;
  private readonly IWebSocketFactory _webSocketFactory;
  private readonly ILogger<DiscordGatewayClient> _logger;
  private readonly IDiscordRestClient _discordRestClient;
  private readonly TimeProvider _timeProvider;
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

  private string _gatewayUrl = string.Empty;
  private IWebSocket? _webSocket;
  private int? _lastSequence;
  private int _heartbeatInterval;
  private DateTimeOffset _timeLastHeartbeatSent = DateTimeOffset.MinValue;
  private DateTimeOffset _timeLastHeartbeatAcknowledged = DateTimeOffset.MinValue;
  private CancellationTokenSource? _heartbeatCts;
  private CancellationTokenSource? _linkedCts;
  private bool _canResume;
  private string _sessionId = string.Empty;
  private string _resumeGatewayUrl = string.Empty;

  public DiscordGatewayClient(
    DiscordClientOptions options,
    IWebSocketFactory webSocketFactory,
    ILogger<DiscordGatewayClient> logger,
    IDiscordRestClient discordRestClient,
    TimeProvider timeProvider
  )
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _webSocketFactory = webSocketFactory ?? throw new ArgumentNullException(nameof(webSocketFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _discordRestClient = discordRestClient ?? throw new ArgumentNullException(nameof(discordRestClient));
    _timeProvider = timeProvider ?? throw new ArgumentNullException(nameof(timeProvider));
  }

  public async Task ConnectAsync(CancellationToken cancellationToken)
  {
    await SetCanResumeAsync(false, cancellationToken);

    if (await IsGatewayUrlSetAsync(cancellationToken) is false)
    {
      var gatewayUrl = await _discordRestClient.GetGatewayUrlAsync(cancellationToken);
      await SetGatewayUrlAsync(gatewayUrl, cancellationToken);
    }

    await SetWebSocketAsync(_webSocketFactory.Create(), cancellationToken);
    await ConnectToGatewayAsync(cancellationToken);

    _logger.LogInformation("Connected to Discord Gateway at {GatewayUrl}", _gatewayUrl);

    _ = ReceiveMessagesAsync(cancellationToken);
  }

  private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
  {
    try
    {
      var messageBuffer = new byte[8192];

      while (cancellationToken.IsCancellationRequested is false)
      {
        // websocket message might be larger than the
        // size of the buffer so we need to loop until
        // we receive the end of the message and
        // write the data we receive on each iteration
        // to the memory stream
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
          result = await ReceiveMessageAsync(new(messageBuffer), cancellationToken);

          if (result.MessageType is WebSocketMessageType.Close)
          {
            var canResume = result.CloseStatus is not WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable;
            await SetCanResumeAsync(canResume, cancellationToken);
            await CloseAsync(result.CloseStatus ?? WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, cancellationToken);
            await ReconnectAsync(cancellationToken);
            return;
          }

          if (result.MessageType is WebSocketMessageType.Text)
          {
            await memoryStream.WriteAsync(messageBuffer.AsMemory(0, result.Count), cancellationToken);
            await memoryStream.FlushAsync(cancellationToken);
          }

        } while (result.EndOfMessage is false);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var e = await JsonSerializer.DeserializeAsync<DiscordEvent>(
          memoryStream,
          _jsonSerializerOptions,
          cancellationToken
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
      _logger.LogInformation(ex, "Receive messages operation canceled: {Message}", ex.Message);
    }
# pragma warning disable CA1031 // Do not catch general exception types
    catch (Exception ex)
# pragma warning restore CA1031 // Do not catch general exception types
    {
      _logger.LogError(ex, "Unexpected error while receiving messages");

      using (await _lock.LockAsync(cancellationToken))
      {
        if (_webSocket?.State is WebSocketState.Open)
        {
          await _webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "WebSocket error. Closing connection and invalidating session.",
            cancellationToken
          );
        }

        _canResume = false;
        await ReconnectAsync(cancellationToken);
      }
    }
  }

  private async Task HandleEventAsync(DiscordEvent e, CancellationToken cancellationToken)
  {
    await SetSequenceAsync(e.Sequence, cancellationToken);

    if (e is HelloDiscordEvent he)
    {
      await SetHeartbeatIntervalAsync(he.Data.HeartbeatInterval, cancellationToken);
      await StartHeartbeatAsync(cancellationToken);
      await IdentifyAsync(cancellationToken);

      _logger.LogInformation("Hello event received. Heartbeat interval: {Interval}", _heartbeatInterval);
      return;
    }

    if (e is HeartbeatAckDiscordEvent hae)
    {
      await SetHeartbeatAcknowledgedAsync(_timeProvider.GetUtcNow(), cancellationToken);
      _logger.LogInformation("Heartbeat acknowledged at {Time}", _timeLastHeartbeatAcknowledged);
      return;
    }

    if (e is ReadyDiscordEvent re)
    {
      await SetSessionIdAsync(re.Data.SessionId, cancellationToken);
      await SetResumeGatewayUrlAsync(re.Data.ResumeGatewayUrl, cancellationToken);

      _logger.LogInformation("Ready event received. Session ID: {SessionId}", _sessionId);
      return;
    }
  }

  private async Task StartHeartbeatAsync(CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      if (_heartbeatCts is not null)
      {
        await _heartbeatCts.CancelAsync();
        _heartbeatCts.Dispose();
      }

      _heartbeatCts = new CancellationTokenSource();
      _linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _heartbeatCts.Token);

      _ = Task.Run(async () =>
      {
        _logger.LogInformation("Starting heartbeat task.");

        while (_linkedCts.Token.IsCancellationRequested is false)
        {
          using (await _lock.LockAsync(_linkedCts.Token))
          {
            if (_webSocket?.State is not WebSocketState.Open)
            {
              _logger.LogWarning("WebSocket is not open. Cannot send heartbeat.");
              break;
            }
          }

          try
          {
# pragma warning disable CA5394 // Do not use insecure randomness
            var jitter = Random.Shared.NextDouble();
# pragma warning restore CA5394 // Do not use insecure randomness
            int heartbeatInterval;

            using (await _lock.LockAsync(_linkedCts.Token))
            {
              heartbeatInterval = _heartbeatInterval + (int)(jitter * _heartbeatInterval);
            }

            await Task.Delay(heartbeatInterval, _linkedCts.Token);

            using (await _lock.LockAsync(_linkedCts.Token))
            {
              if (_timeLastHeartbeatAcknowledged < _timeLastHeartbeatSent)
              {
                if (_webSocket?.State is WebSocketState.Open)
                {
                  _logger.LogWarning("Heartbeat not acknowledged. Closing WebSocket.");

                  await _webSocket.CloseAsync(
                    WebSocketCloseStatus.ProtocolError,
                    "Heartbeat not acknowledged",
                    _linkedCts.Token
                  );

                  _canResume = true;
                }

                await ReconnectAsync(cancellationToken);

                break;
              }

              await SendHeartbeatAsync(_linkedCts.Token);
            }

            _logger.LogInformation("Heartbeat sent at {Time}", _timeLastHeartbeatSent);
          }
          catch (OperationCanceledException ex)
          {
            _logger.LogInformation(ex, "Heartbeat task canceled");
          }
# pragma warning disable CA1031 // Do not catch general exception types
          catch (Exception ex)
# pragma warning restore CA1031 // Do not catch general exception types
          {
            _logger.LogError(ex, "Error in heartbeat task: {Message}", ex.Message);
          }
        }
      }, _linkedCts.Token);
    }
  }

  private async Task ReconnectAsync(CancellationToken token)
  {
    _webSocket?.Dispose();

    if (_heartbeatCts is not null)
    {
      await _heartbeatCts.CancelAsync();
      _heartbeatCts.Dispose();
    }

    if (_linkedCts is not null)
    {
      await _linkedCts.CancelAsync();
      _linkedCts.Dispose();
    }

    if (_canResume)
    {
      _logger.LogInformation("Resuming connection to Discord Gateway.");

      _webSocket = _webSocketFactory.Create();
      var uri = new Uri(_resumeGatewayUrl);
      await _webSocket.ConnectAsync(uri, token);
      await SendResumeAsync(token);
      _ = ReceiveMessagesAsync(token);
      _ = StartHeartbeatAsync(token);
      return;
    }

    _logger.LogInformation("Reconnecting to Discord Gateway.");
    await ConnectAsync(token);
  }

  private async Task SendResumeAsync(CancellationToken cancellationToken)
  {
    if (_lastSequence is null)
    {
      throw new InvalidOperationException("Cannot resume without a sequence number.");
    }

    var resume = new ResumeDiscordEvent(
      _options.AppToken,
      _sessionId,
      _lastSequence.Value
    );

    await SendJsonAsync(resume, cancellationToken);
  }

  private async Task IdentifyAsync(CancellationToken cancellationToken)
  {
    var identify = new IdentifyDiscordEvent(
      _options.AppToken,
      _options.Intents
    );

    await SendJsonAsync(identify, cancellationToken);
  }

  private async Task SendHeartbeatAsync(CancellationToken cancellationToken)
  {
    var heartbeat = new HeartbeatDiscordEvent(_lastSequence);
    await SendJsonAsync(heartbeat, cancellationToken);
    await SetHeartbeatSentAsync(_timeProvider.GetUtcNow(), cancellationToken);
  }

  private async Task SendJsonAsync(object data, CancellationToken cancellationToken)
  {
    if (_webSocket?.State is not WebSocketState.Open)
    {
      _logger.LogWarning("WebSocket is not open. Cannot send message.");
      return;
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

  private async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      if (_webSocket is null)
      {
        throw new InvalidOperationException("WebSocket is not set. Cannot close.");
      }

      if (_webSocket.State is not WebSocketState.Open)
      {
        throw new InvalidOperationException("WebSocket is not open. Cannot close.");
      }

      await _webSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
    }
  }

  private async Task<WebSocketReceiveResult> ReceiveMessageAsync(
    ArraySegment<byte> segment,
    CancellationToken cancellationToken
  )
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      if (_webSocket is null)
      {
        throw new InvalidOperationException("WebSocket is not set. Cannot receive message.");
      }

      if (_webSocket.State is not WebSocketState.Open)
      {
        throw new InvalidOperationException("WebSocket is not open. Cannot receive message.");
      }
      var result = await _webSocket.ReceiveAsync(segment, cancellationToken);
      return result;
    }
  }

  private async Task ConnectToGatewayAsync(CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      if (_webSocket is null)
      {
        throw new InvalidOperationException("WebSocket is not set. Cannot connect.");
      }

      if (_webSocket.State is WebSocketState.Open)
      {
        throw new InvalidOperationException("WebSocket is already open. Cannot connect.");
      }

      await _webSocket.ConnectAsync(new Uri(_gatewayUrl), cancellationToken);
    }
  }

  private async Task<bool> IsGatewayUrlSetAsync(CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      return !string.IsNullOrEmpty(_gatewayUrl);
    }
  }

  private async Task SetGatewayUrlAsync(string url, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _gatewayUrl = url;
    }
  }

  private async Task SetSequenceAsync(int? sequence, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _lastSequence = sequence;
    }
  }

  private async Task SetHeartbeatIntervalAsync(int interval, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _heartbeatInterval = interval;
    }
  }

  private async Task SetHeartbeatSentAsync(DateTimeOffset time, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _timeLastHeartbeatSent = time;
    }
  }

  private async Task SetHeartbeatAcknowledgedAsync(DateTimeOffset time, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _timeLastHeartbeatAcknowledged = time;
    }
  }

  private async Task SetSessionIdAsync(string sessionId, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _sessionId = sessionId;
    }
  }

  private async Task SetResumeGatewayUrlAsync(string url, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _resumeGatewayUrl = url;
    }
  }

  private async Task<bool> IsWebSocketOpenAsync(CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      return _webSocket?.State is WebSocketState.Open;
    }
  }

  private async Task SetWebSocketAsync(IWebSocket webSocket, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _webSocket = webSocket;
    }
  }

  private async Task SetCanResumeAsync(bool canResume, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _canResume = canResume;
    }
  }

  public void Dispose()
  {
    _heartbeatCts?.Cancel();
    _linkedCts?.Cancel();

    _heartbeatCts?.Dispose();
    _linkedCts?.Dispose();
    _webSocket?.Dispose();
    _lock.Dispose();
  }
}