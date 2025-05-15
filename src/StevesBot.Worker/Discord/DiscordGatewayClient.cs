using System.Text;

namespace StevesBot.Worker.Discord;

internal sealed class DiscordGatewayClient : IDiscordGatewayClient
{
  private readonly DiscordClientOptions _options;
  private readonly IWebSocketFactory _webSocketFactory;
  private readonly ILogger<DiscordGatewayClient> _logger;
  private readonly IDiscordRestClient _discordRestClient;
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
  private DateTime _timeLastHeartbeatSent = DateTime.MinValue;
  private DateTime _timeLastHeartbeatAcknowledged = DateTime.MinValue;
  private CancellationTokenSource? _heartbeatCts;
  private CancellationTokenSource? _linkedCts;
  private bool _canResume;
  private string _sessionId = string.Empty;
  private string _resumeGatewayUrl = string.Empty;

  public DiscordGatewayClient(
    DiscordClientOptions options,
    IWebSocketFactory webSocketFactory,
    ILogger<DiscordGatewayClient> logger,
    IDiscordRestClient discordRestClient
  )
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _webSocketFactory = webSocketFactory ?? throw new ArgumentNullException(nameof(webSocketFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    _discordRestClient = discordRestClient ?? throw new ArgumentNullException(nameof(discordRestClient));
  }

  public async Task ConnectAsync(CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      if (string.IsNullOrEmpty(_gatewayUrl))
      {
        _gatewayUrl = await _discordRestClient.GetGatewayUrlAsync(cancellationToken);
      }

      _webSocket = _webSocketFactory.Create();

      var uri = new Uri(_gatewayUrl);
      await _webSocket.ConnectAsync(uri, cancellationToken);

      _logger.LogInformation("Connected to Discord Gateway at {GatewayUrl}", _gatewayUrl);

      _ = ReceiveMessagesAsync(cancellationToken);
    }
  }

  private async Task ReceiveMessagesAsync(CancellationToken cancellationToken)
  {
    try
    {
      var messageBuffer = new byte[8192];

      while (cancellationToken.IsCancellationRequested is false)
      {
        using (await _lock.LockAsync(cancellationToken))
        {
          if (_webSocket?.State is not WebSocketState.Open)
          {
            _logger.LogWarning("WebSocket is not open. Cannot receive messages.");
            return;
          }
        }

        if (_webSocket?.State is not WebSocketState.Open)
        {
          _logger.LogWarning("WebSocket is not open. Cannot receive messages.");
          return;
        }

        // websocket message might be larger than the
        // size of the buffer so we need to loop until
        // we receive the end of the message and
        // write the data we receive on each iteration
        // to the memory stream
        using var memoryStream = new MemoryStream();
        WebSocketReceiveResult result;

        do
        {
          result = await _webSocket.ReceiveAsync(new(messageBuffer), cancellationToken);

          if (result.MessageType is WebSocketMessageType.Close)
          {
            // TODO: If close status is 1000 or 1001 we cannot resume.
            // if 1000 or 1001 we should close the connection and reconnect
            // else we should close the connection and attempt to resume

            if (result.CloseStatus is WebSocketCloseStatus.NormalClosure or WebSocketCloseStatus.EndpointUnavailable)
            {
              return;
            }

            return;
          }

          if (result.MessageType is WebSocketMessageType.Text)
          {
            await memoryStream.WriteAsync(messageBuffer.AsMemory(0, result.Count), cancellationToken);
            await memoryStream.FlushAsync(cancellationToken);
          }

        } while (result.EndOfMessage is false);

        memoryStream.Seek(0, SeekOrigin.Begin);

        var message = Encoding.UTF8.GetString(messageBuffer, 0, result.Count);
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
    catch (WebSocketException ex)
    {
      _logger.LogError(ex, "WebSocket error");
      throw new DiscordGatewayClientException("WebSocket error.", ex);
    }
    catch (OperationCanceledException ex)
    {
      _logger.LogInformation(ex, "Receive messages operation canceled: {Message}", ex.Message);
    }
    catch (Exception ex)
    {
      _logger.LogError(ex, "Unexpected error while receiving messages");
      throw new DiscordGatewayClientException("Unexpected error while receiving messages.", ex);
    }
  }

  private async Task HandleEventAsync(DiscordEvent e, CancellationToken cancellationToken)
  {
    using (await _lock.LockAsync(cancellationToken))
    {
      _lastSequence = e.Sequence;
    }

    if (e is HelloDiscordEvent he)
    {
      using (await _lock.LockAsync(cancellationToken))
      {
        _heartbeatInterval = he.Data.HeartbeatInterval;
      }

      await StartHeartbeatAsync(cancellationToken);
      return;
    }

    if (e is HeartbeatAckDiscordEvent hae)
    {
      using (await _lock.LockAsync(cancellationToken))
      {
        _timeLastHeartbeatAcknowledged = DateTime.UtcNow;
      }

      _logger.LogInformation("Heartbeat acknowledged at {Time}", _timeLastHeartbeatAcknowledged);
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

                await ReconnectAsync(_linkedCts.Token);

                break;
              }

              _timeLastHeartbeatSent = await SendHeartbeatAsync(_linkedCts.Token);
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
            // TODO: Attempt to reconnect
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

      _canResume = false;
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

  private async Task<DateTime> SendHeartbeatAsync(CancellationToken cancellationToken)
  {
    var heartbeat = new HeartbeatDiscordEvent(_lastSequence);
    await SendJsonAsync(heartbeat, cancellationToken);
    return DateTime.UtcNow;
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