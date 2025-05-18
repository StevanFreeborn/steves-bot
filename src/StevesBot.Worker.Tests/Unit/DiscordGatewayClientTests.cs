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
  public async Task ConnectAsync_WhenCalledAndWebSocketIsClosed_ItShouldStopReceivingMessages()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(WebSocketState.Closed);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    await _discordGatewayClient.ConnectAsync(CancellationToken.None);

    mockWebSocket.Verify(
      static x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()),
      Times.Never
    );
  }

  [Fact]
  public async Task ConnectAsync_WhenCalledAndCancellationIsRequested_ItShouldStopReceivingMessages()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(100);
    await cts.CancelAsync();

    mockWebSocket.Verify(static x => x.State, Times.AtMostOnce);
    mockWebSocket.Verify(
      static x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()),
      Times.AtMostOnce
    );
  }

  [Fact]
  public async Task ConnectAsync_WhenCalledAndHelloEventReceived_ItShouldStartSendingHeartbeats()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(WebSocketState.Open);

    var messageQueue = new Queue<(WebSocketReceiveResult, byte[])>();

    var heartbeatInterval = 100;

    var helloEventPayload = CreateEventPayload(new
    {
      op = 10,
      d = new
      {
        heartbeat_interval = heartbeatInterval,
      }
    });

    messageQueue.Enqueue((
      new(helloEventPayload.Bytes.Length, WebSocketMessageType.Text, true),
      helloEventPayload.Bytes
    ));

    SetupReceiveMessageSequence(mockWebSocket, messageQueue);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    var cts = new CancellationTokenSource();
    await _discordGatewayClient.ConnectAsync(cts.Token);

    await Task.Delay((int)(heartbeatInterval * 1.5));
    await cts.CancelAsync();

    var expectedHeartbeatPayload = CreateEventPayload(new HeartbeatDiscordEvent(null));

    mockWebSocket.Verify(
      x => x.SendAsync(
        It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b)),
        It.IsAny<WebSocketMessageType>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ConnectAsync_WhenCalledAndHeartbeatIsNotAcknowledged_ItShouldDisconnectAndAttemptToResume()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .Setup(static x => x.State)
      .Returns(WebSocketState.Open);

    var messageQueue = new Queue<(WebSocketReceiveResult, byte[])>();

    var heartbeatInterval = 100;

    var helloEventPayload = CreateEventPayload(new
    {
      op = 10,
      d = new
      {
        heartbeat_interval = heartbeatInterval,
      }
    });

    messageQueue.Enqueue((
      new(helloEventPayload.Bytes.Length, WebSocketMessageType.Text, true),
      helloEventPayload.Bytes
    ));

    SetupReceiveMessageSequence(mockWebSocket, messageQueue);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    var cts = new CancellationTokenSource();
    await _discordGatewayClient.ConnectAsync(cts.Token);

    await Task.Delay((int)(heartbeatInterval * 2.5));
    await cts.CancelAsync();

    var expectedHeartbeatPayload = CreateEventPayload(new HeartbeatDiscordEvent(null));

    mockWebSocket.Verify(
      x => x.SendAsync(
        It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b)),
        It.IsAny<WebSocketMessageType>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Once
    );

    mockWebSocket.Verify(
      static x => x.CloseAsync(
        It.Is<WebSocketCloseStatus>(
          x => x != WebSocketCloseStatus.NormalClosure && x != WebSocketCloseStatus.EndpointUnavailable
        ),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Once
    );
  }

  [Fact]
  public async Task ConnectAsync_WhenCalledAndHeartbeatNotAcknowlegedAndAlreadyClosed_ItShouldNotDisconnect()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var mockWebSocket = new Mock<IWebSocket>();

    mockWebSocket
      .SetupSequence(static x => x.State)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Open)
      .Returns(WebSocketState.Closed);

    var messageQueue = new Queue<(WebSocketReceiveResult, byte[])>();

    var heartbeatInterval = 100;

    var helloEventPayload = CreateEventPayload(new
    {
      op = 10,
      d = new
      {
        heartbeat_interval = heartbeatInterval,
      }
    });

    messageQueue.Enqueue((
      new(helloEventPayload.Bytes.Length, WebSocketMessageType.Text, true),
      helloEventPayload.Bytes
    ));

    SetupReceiveMessageSequence(mockWebSocket, messageQueue);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    var cts = new CancellationTokenSource();
    await _discordGatewayClient.ConnectAsync(cts.Token);

    await Task.Delay((int)(heartbeatInterval * 2.5));
    await cts.CancelAsync();

    var expectedHeartbeatPayload = CreateEventPayload(new HeartbeatDiscordEvent(null));

    mockWebSocket.Verify(
      x => x.SendAsync(
        It.Is<ArraySegment<byte>>(b => expectedHeartbeatPayload.Bytes.SequenceEqual(b)),
        It.IsAny<WebSocketMessageType>(),
        It.IsAny<bool>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Once
    );

    mockWebSocket.Verify(
      static x => x.CloseAsync(
        It.IsAny<WebSocketCloseStatus>(),
        It.IsAny<string>(),
        It.IsAny<CancellationToken>()
      ),
      Times.Never
    );
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