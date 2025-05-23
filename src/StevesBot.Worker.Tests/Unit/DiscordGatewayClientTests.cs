using System.ComponentModel;

namespace StevesBot.Worker.Tests.Unit;

public sealed class DiscordGatewayClientTests : IDisposable
{
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<IWebSocketFactory> _mockWebSocketFactory = new();
  private readonly Mock<ILogger<DiscordGatewayClient>> _mockLogger = new();
  private readonly Mock<TimeProvider> _mockTimeProvider = new();
  private readonly Mock<IServiceScopeFactory> _mockServiceScopeFactory = new();
  private readonly DiscordClientOptions _options = new();
  private readonly DiscordGatewayClient _discordGatewayClient;

  public DiscordGatewayClientTests()
  {
    _mockTimeProvider
      .Setup(static x => x.GetUtcNow())
      .Returns(DateTimeOffset.UtcNow);

    _discordGatewayClient = new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      _mockDiscordRestClient.Object,
      _mockTimeProvider.Object,
      _mockServiceScopeFactory.Object
    );
  }

  [Fact]
  public void Constructor_WhenCalledAndOptionsIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      null!,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      _mockDiscordRestClient.Object,
      _mockTimeProvider.Object,
      _mockServiceScopeFactory.Object
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenCalledAndWebSocketFactoryIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      _options,
      null!,
      _mockLogger.Object,
      _mockDiscordRestClient.Object,
      _mockTimeProvider.Object,
      _mockServiceScopeFactory.Object
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenCalledAndLoggerIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      null!,
      _mockDiscordRestClient.Object,
      _mockTimeProvider.Object,
      _mockServiceScopeFactory.Object
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenCalledAndDiscordRestClientIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      null!,
      _mockTimeProvider.Object,
      _mockServiceScopeFactory.Object
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenCalledAndTimeProviderIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      _mockDiscordRestClient.Object,
      null!,
      _mockServiceScopeFactory.Object
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenCalledAndServiceScopeFactoryIsNull_ItShouldThrowArgumentNullException()
  {
    var act = () => new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      _mockDiscordRestClient.Object,
      _mockTimeProvider.Object,
      null!
    );

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public async Task ConnectAsync_WhenCalled_ItShouldConnectToGateway()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .SetupSequence(static x => x.State)
      .Returns(WebSocketState.Closed)
      .Returns(WebSocketState.Open);

    mockWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    await _discordGatewayClient.ConnectAsync(CancellationToken.None);

    mockWebSocket
      .Verify(
        static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()),
        Times.Once
      );
  }

  [Fact]
  public async Task ConnectAsync_WhenConnectedAndHelloEventIsReceived_ItShouldStartSendingHeartbeatsAndIdentify()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    var socketState = WebSocketState.Closed;

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(() => socketState);

    mockWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => socketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    var messagesToReceive = new Queue<(WebSocketReceiveResult, byte[])>();
    var heartbeatInterval = 100;
    var helloEvent = new HelloDiscordEvent()
    {
      Data = new()
      {
        HeartbeatInterval = heartbeatInterval,
      }
    };
    var helloPayload = CreateEventPayload(helloEvent);
    var helloResult = new WebSocketReceiveResult(helloPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((helloResult, helloPayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay((int)(heartbeatInterval * 1.5));
    await cts.CancelAsync();

    var expectedHeartbeatEvent = new HeartbeatDiscordEvent(helloEvent.Sequence);
    var expectedHeartbeatPayload = CreateEventPayload(expectedHeartbeatEvent);

    mockWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );

    var expectedIdentifyEvent = new IdentifyDiscordEvent(
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
    var expectedIdentifyPayload = CreateEventPayload(expectedIdentifyEvent);

    mockWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedIdentifyPayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task ConnectAsync_OnceConnectedIfHeartbeatIsNotAcknowledged_ItShouldStopSendingHeartbeatsAndAttemptToResume()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var initialSocketState = WebSocketState.Closed;

    var initialWebSocket = new Mock<IWebSocket>();

    initialWebSocket
      .Setup(static x => x.State)
      .Returns(() => initialSocketState);

    initialWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    initialWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialSocketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var resumingSocketState = WebSocketState.Closed;

    var resumingWebSocket = new Mock<IWebSocket>();

    resumingWebSocket
      .Setup(static x => x.State)
      .Returns(() => resumingSocketState);

    resumingWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumingSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    resumingWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumingSocketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var messagesToReceive = new Queue<(WebSocketReceiveResult, byte[])>();
    var heartbeatInterval = 100;
    var helloEvent = new HelloDiscordEvent()
    {
      Data = new()
      {
        HeartbeatInterval = heartbeatInterval,
      }
    };
    var helloPayload = CreateEventPayload(helloEvent);
    var helloResult = new WebSocketReceiveResult(helloPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((helloResult, helloPayload.Bytes));

    var sessionId = "session_id";
    var resumeGatewayUrl = "wss://resume.discord.gg";
    var readyEvent = new ReadyDiscordEvent()
    {
      Sequence = 1,
      Data = new ReadyData()
      {
        SessionId = sessionId,
        ResumeGatewayUrl = resumeGatewayUrl,
      }
    };
    var readyPayload = CreateEventPayload(readyEvent);
    var readyResult = new WebSocketReceiveResult(readyPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((readyResult, readyPayload.Bytes));

    SetupReceiveMessageSequence(initialWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .SetupSequence(static x => x.Create())
      .Returns(initialWebSocket.Object)
      .Returns(resumingWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay((int)(heartbeatInterval * 2.5));
    await cts.CancelAsync();

    var expectedHeartbeatEvent = new HeartbeatDiscordEvent(helloEvent.Sequence);
    var expectedHeartbeatPayload = CreateEventPayload(expectedHeartbeatEvent);

    initialWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.AtMostOnce
      );

    var expectedUri = new Uri($"{resumeGatewayUrl}/?v=10&encoding=json");

    resumingWebSocket
      .Verify(
        x => x.ConnectAsync(
          It.Is<Uri>(uri => uri.Equals(expectedUri)),
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );
  }

  [Fact]
  public async Task ConnectAsync_OnceConnectedIfHeartbeatIsAcknowledged_ItShouldContinueSendingHeartbeats()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    var socketState = WebSocketState.Closed;

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(() => socketState);

    mockWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => socketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    var messagesToReceive = new Queue<(WebSocketReceiveResult, byte[])>();
    var heartbeatInterval = 100;
    var helloEvent = new HelloDiscordEvent()
    {
      Data = new()
      {
        HeartbeatInterval = heartbeatInterval,
      }
    };
    var helloPayload = CreateEventPayload(helloEvent);
    var helloResult = new WebSocketReceiveResult(helloPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((helloResult, helloPayload.Bytes));

    var heartbeatAckEvent = new HeartbeatAckDiscordEvent();
    var heartbeatAckPayload = CreateEventPayload(heartbeatAckEvent);
    var heartbeatAckResult = new WebSocketReceiveResult(heartbeatAckPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((heartbeatAckResult, heartbeatAckPayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay((int)(heartbeatInterval * 2.5));
    await cts.CancelAsync();

    var expectedHeartbeatEvent = new HeartbeatDiscordEvent(helloEvent.Sequence);
    var expectedHeartbeatPayload = CreateEventPayload(expectedHeartbeatEvent);

    mockWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.AtLeast(2)
      );
  }

  [Fact]
  public async Task ConnectAsync_OnceConnectedWhenHeartRequestReceived_ItShouldImmediatelySendHeartbeat()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var socketState = WebSocketState.Closed;

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(() => socketState);

    mockWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => socketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    mockWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => socketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var messagesToReceive = new Queue<(WebSocketReceiveResult, byte[])>();
    var heartbeatInterval = 100;
    var helloEvent = new HelloDiscordEvent()
    {
      Data = new()
      {
        HeartbeatInterval = heartbeatInterval,
      }
    };
    var helloPayload = CreateEventPayload(helloEvent);
    var helloResult = new WebSocketReceiveResult(helloPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((helloResult, helloPayload.Bytes));

    var sessionId = "session_id";
    var resumeGatewayUrl = "wss://resume.discord.gg";
    var readyEvent = new ReadyDiscordEvent()
    {
      Data = new ReadyData()
      {
        SessionId = sessionId,
        ResumeGatewayUrl = resumeGatewayUrl,
      }
    };
    var readyPayload = CreateEventPayload(readyEvent);
    var readyResult = new WebSocketReceiveResult(readyPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((readyResult, readyPayload.Bytes));

    var heartbeatAckEvent = new HeartbeatAckDiscordEvent();
    var heartbeatAckPayload = CreateEventPayload(heartbeatAckEvent);
    var heartbeatAckResult = new WebSocketReceiveResult(heartbeatAckPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((heartbeatAckResult, heartbeatAckPayload.Bytes));

    var heartbeatRequestEvent = new HeartbeatDiscordEvent(readyEvent.Sequence);
    var heartbeatRequestPayload = CreateEventPayload(heartbeatRequestEvent);
    var heartbeatRequestResult = new WebSocketReceiveResult(heartbeatRequestPayload.Bytes.Length, WebSocketMessageType.Text, true);
    messagesToReceive.Enqueue((heartbeatRequestResult, heartbeatRequestPayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    var expectedHeartbeatEvent = new HeartbeatDiscordEvent(helloEvent.Sequence);
    var expectedHeartbeatPayload = CreateEventPayload(expectedHeartbeatEvent);

    mockWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.AtLeast(2)
      );
  }

  [Fact]
  public Task ConnectAsync_OnceConnectedWhenUnReconnectableCloseStatusIsReceived_ItShouldDisconnectAndNotTryToReconnect()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public Task ConnectAsync_OnceConnectedWhenReconnectableButNonResumableCloseStatusIsReceived_ItShouldReconnect()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public Task ConnectAsync_OnceConnectedWhenResumableCloseStatusIsReceived_ItShouldResume()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public Task ConnectAsync_OnceConnectedWhenReconnectEventIsReceivedThatIndicatesClientCanResume_ItShouldResume()
  {
    return Task.CompletedTask;
  }

  [Fact]
  public Task ConnectAsync_OnceConnectedWhenReconnectEventIsReceivedThatIndicatesClientCannotResume_ItShouldReconnect()
  {
    return Task.CompletedTask;
  }

  private static void SetupReceiveMessageSequence(
    Mock<IWebSocket> mockWebSocket,
    Queue<(WebSocketReceiveResult, byte[])> messageQueue
  )
  {
    mockWebSocket
      .Setup(static x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
      .Returns((ArraySegment<byte> buffer, CancellationToken token) =>
      {
        if (messageQueue.Count == 0)
        {
          return Task.Delay(-1, token)
            .ContinueWith(
              _ => new WebSocketReceiveResult(0, WebSocketMessageType.Text, true),
              TaskScheduler.Default
            );
        }

        var (result, messageBytes) = messageQueue.Dequeue();
        Array.Copy(messageBytes, 0, buffer.Array!, buffer.Offset, Math.Min(messageBytes.Length, buffer.Count));
        return Task.FromResult(result);
      });
  }

  private static (byte[] Bytes, string Json) CreateEventPayload(object e)
  {
    var json = JsonSerializer.Serialize(e);
    var bytes = Encoding.UTF8.GetBytes(json);
    return (bytes, json);
  }

  public void Dispose()
  {
    _discordGatewayClient.Dispose();
  }
}