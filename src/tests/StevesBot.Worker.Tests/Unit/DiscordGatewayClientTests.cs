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

    var mockServiceProvider = new Mock<IServiceProvider>();

    var mockServiceScope = new Mock<IServiceScope>();

    mockServiceScope
      .Setup(static x => x.ServiceProvider)
      .Returns(mockServiceProvider.Object);

    _mockServiceScopeFactory
      .Setup(static x => x.CreateScope())
      .Returns(mockServiceScope.Object);

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
        Times.AtLeastOnce
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
  public async Task ConnectAsync_OnceConnectedWhenUnReconnectableCloseStatusIsReceived_ItShouldDisconnectAndNotTryToReconnect()
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

    var closeEvent = new DiscordEvent();
    var closePayload = CreateEventPayload(closeEvent);
    var closeResult = new WebSocketReceiveResult(
      closePayload.Bytes.Length,
      WebSocketMessageType.Close,
      true,
      (WebSocketCloseStatus)4004,
      "Closing"
    );
    messagesToReceive.Enqueue((closeResult, closePayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    mockWebSocket
      .Verify(
        x => x.CloseAsync(
          It.Is<WebSocketCloseStatus>(s => s == WebSocketCloseStatus.NormalClosure),
          It.IsAny<string>(),
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );
  }

  [Theory]
  [InlineData("", "session_id", 1010, 0)]
  [InlineData("wss://resume.discord.gg", "", 1010, 0)]
  [InlineData("wss://resume.discord.gg", "session_id", 1010, 1)]
  [InlineData("wss://resume.discord.gg", "session_id", 1001, 0)]
  public async Task ConnectAsync_OnceConnectedWhenCloseStatusIsReceived_ItShouldResumeOrReconnectCorrectly(
    string resumeGatewayUrl,
    string sessionId,
    int closureStatus,
    int resumeCount
  )
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var initialWebSocketState = WebSocketState.Closed;

    var initialWebSocket = new Mock<IWebSocket>();

    initialWebSocket
      .Setup(static x => x.State)
      .Returns(() => initialWebSocketState);

    initialWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    initialWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var resumeWebSocketState = WebSocketState.Closed;

    var resumingWebSocket = new Mock<IWebSocket>();

    resumingWebSocket
      .Setup(static x => x.State)
      .Returns(() => resumeWebSocketState);

    resumingWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    resumingWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Closed)
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

    var closeEvent = new DiscordEvent();
    var closePayload = CreateEventPayload(closeEvent);
    var closeResult = new WebSocketReceiveResult(
      closePayload.Bytes.Length,
      WebSocketMessageType.Close,
      true,
      (WebSocketCloseStatus)closureStatus,
      "Closing"
    );
    messagesToReceive.Enqueue((closeResult, closePayload.Bytes));

    SetupReceiveMessageSequence(initialWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .SetupSequence(static x => x.Create())
      .Returns(initialWebSocket.Object)
      .Returns(resumingWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    var expectedUri = new Uri("wss://resume.discord.gg/?v=10&encoding=json");

    resumingWebSocket
      .Verify(
        x => x.ConnectAsync(
          It.Is<Uri>(uri => uri.Equals(expectedUri)),
          It.IsAny<CancellationToken>()
        ),
        Times.Exactly(resumeCount)
      );
  }

  [Fact]
  public async Task ConnectAsync_OnceConnectedWhenReconnectEventIsReceived_ItShouldResume()
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var initialWebSocketState = WebSocketState.Closed;

    var initialWebSocket = new Mock<IWebSocket>();

    initialWebSocket
      .Setup(static x => x.State)
      .Returns(() => initialWebSocketState);

    initialWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    initialWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var resumeWebSocketState = WebSocketState.Closed;

    var resumingWebSocket = new Mock<IWebSocket>();

    resumingWebSocket
      .Setup(static x => x.State)
      .Returns(() => resumeWebSocketState);

    resumingWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    resumingWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Closed)
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

    var reconnectEvent = new ReconnectDiscordEvent();
    var reconnectPayload = CreateEventPayload(reconnectEvent);
    var reconnectResult = new WebSocketReceiveResult(
      reconnectPayload.Bytes.Length,
      WebSocketMessageType.Text,
      true
    );
    messagesToReceive.Enqueue((reconnectResult, reconnectPayload.Bytes));

    SetupReceiveMessageSequence(initialWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .SetupSequence(static x => x.Create())
      .Returns(initialWebSocket.Object)
      .Returns(resumingWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

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

  [Theory]
  [InlineData(true, 1)]
  [InlineData(false, 0)]
  public async Task ConnectAsync_OnceConnectedWhenInvalidSessionEventIsReceived_ItShouldResumeOrReconnectCorrectly(
    bool canResume,
    int resumeCount
  )
  {
    _mockDiscordRestClient
      .Setup(static x => x.GetGatewayUrlAsync(It.IsAny<CancellationToken>()))
      .ReturnsAsync("wss://gateway.discord.gg");

    var initialWebSocketState = WebSocketState.Closed;

    var initialWebSocket = new Mock<IWebSocket>();

    initialWebSocket
      .Setup(static x => x.State)
      .Returns(() => initialWebSocketState);

    initialWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    initialWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => initialWebSocketState = WebSocketState.Closed)
      .Returns(Task.CompletedTask);

    var resumeWebSocketState = WebSocketState.Closed;

    var resumingWebSocket = new Mock<IWebSocket>();

    resumingWebSocket
      .Setup(static x => x.State)
      .Returns(() => resumeWebSocketState);

    resumingWebSocket
      .Setup(static x => x.ConnectAsync(It.IsAny<Uri>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Open)
      .Returns(Task.CompletedTask);

    resumingWebSocket
      .Setup(static x => x.CloseAsync(It.IsAny<WebSocketCloseStatus>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
      .Callback(() => resumeWebSocketState = WebSocketState.Closed)
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

    var invalidSessionEvent = new InvalidSessionDiscordEvent()
    {
      Data = canResume,
    };
    var invalidSessionPayload = CreateEventPayload(invalidSessionEvent);
    var invalidSessionResult = new WebSocketReceiveResult(
      invalidSessionPayload.Bytes.Length,
      WebSocketMessageType.Text,
      true
    );
    messagesToReceive.Enqueue((invalidSessionResult, invalidSessionPayload.Bytes));

    SetupReceiveMessageSequence(initialWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .SetupSequence(static x => x.Create())
      .Returns(initialWebSocket.Object)
      .Returns(resumingWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    var expectedUri = new Uri($"{resumeGatewayUrl}/?v=10&encoding=json");

    resumingWebSocket
      .Verify(
        x => x.ConnectAsync(
          It.Is<Uri>(uri => uri.Equals(expectedUri)),
          It.IsAny<CancellationToken>()
        ),
        Times.Exactly(resumeCount)
      );
  }

  [Fact]
  public async Task DisconnectAsync_WhenCalled_ItShouldDisconnectAndSendIdleStatus()
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

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    var now = DateTime.UtcNow;

    _mockTimeProvider
      .Setup(static x => x.GetUtcNow())
      .Returns(now);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);

    await _discordGatewayClient.DisconnectAsync(cts.Token);

    var expectedIdleEvent = new UpdatePresenceDiscordEvent(
      now.Millisecond,
      [
        new Activity
        {
          Name = "Taking a break. Stevan's got this.",
          Type = ActivityType.Custom,
          State = "Taking a break. Stevan's got this.",
        }
      ],
      PresenceStatus.Idle,
      true
    );
    var expectedIdlePayload = CreateEventPayload(expectedIdleEvent);

    mockWebSocket
      .Verify(
        x => x.SendAsync(
          It.Is<ArraySegment<byte>>(b => expectedIdlePayload.Bytes.SequenceEqual(b.Array!)),
          It.Is<WebSocketMessageType>(m => m == WebSocketMessageType.Text),
          true,
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );

    mockWebSocket
      .Verify(
        static x => x.CloseAsync(WebSocketCloseStatus.NormalClosure, It.IsAny<string>(), It.IsAny<CancellationToken>()),
        Times.Once
      );
  }

  [Fact]
  public void On_WhenCalledWithInvalidEvent_ItShouldThrowException()
  {
    var handler = new Func<DiscordEvent, IServiceProvider, CancellationToken, Task>((e, sp, ct) => Task.CompletedTask);
    var act = () => _discordGatewayClient.On("made_it_up", handler);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void On_WhenCalledWithNullHandler_ItShouldThrowException()
  {
    var act = () => _discordGatewayClient.On(DiscordEventTypes.Ready, null!);
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public async Task On_WhenCalledWithValidEventAndHandler_ItShouldSubscribeHandlerToEvent()
  {
    var readyHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();
    var firstCreateMessageHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();
    var secondCreateMessageHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();

    secondCreateMessageHandler
      .Setup(static x => x(It.IsAny<DiscordEvent>(), It.IsAny<IServiceProvider>(), It.IsAny<CancellationToken>()))
      .ThrowsAsync(new Exception("This is a test exception"));

    _discordGatewayClient.On(DiscordEventTypes.Ready, readyHandler.Object);
    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, firstCreateMessageHandler.Object);
    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, secondCreateMessageHandler.Object);

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

    var messageCreateEvent = new MessageCreateDiscordEvent();
    var messageCreatePayload = CreateEventPayload(messageCreateEvent);
    var messageCreateResult = new WebSocketReceiveResult(
      messageCreatePayload.Bytes.Length,
      WebSocketMessageType.Text,
      true
    );
    messagesToReceive.Enqueue((messageCreateResult, messageCreatePayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();
    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    readyHandler.Invocations.Should().HaveCount(1);
    firstCreateMessageHandler.Invocations.Should().HaveCount(1);
    secondCreateMessageHandler.Invocations.Should().HaveCount(1);
  }

  [Fact]
  public void Off_WhenCalledWithInvalidEvent_ItShouldThrowException()
  {
    var handler = new Func<DiscordEvent, IServiceProvider, CancellationToken, Task>((e, sp, ct) => Task.CompletedTask);
    var act = () => _discordGatewayClient.Off("made_it_up", handler);
    act.Should().Throw<ArgumentException>();
  }

  [Fact]
  public void Off_WhenCalledWithNullHandler_ItShouldThrowException()
  {
    var act = () => _discordGatewayClient.Off(DiscordEventTypes.Ready, null!);
    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public async Task Off_WhenCalledWithValidEventAndHandler_ItShouldUnsubscribeHandlerFromEvent()
  {
    var readyHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();
    var firstCreateMessageHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();
    var secondCreateMessageHandler = new Mock<Func<DiscordEvent, IServiceProvider, CancellationToken, Task>>();

    _discordGatewayClient.On(DiscordEventTypes.Ready, readyHandler.Object);
    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, firstCreateMessageHandler.Object);
    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, secondCreateMessageHandler.Object);

    _discordGatewayClient.Off(DiscordEventTypes.MessageCreate, firstCreateMessageHandler.Object);
    _discordGatewayClient.Off(DiscordEventTypes.Ready, readyHandler.Object);
    _discordGatewayClient.Off(DiscordEventTypes.Ready, readyHandler.Object);

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

    var messageCreateEvent = new MessageCreateDiscordEvent();
    var messageCreatePayload = CreateEventPayload(messageCreateEvent);
    var messageCreateResult = new WebSocketReceiveResult(
      messageCreatePayload.Bytes.Length,
      WebSocketMessageType.Text,
      true
    );
    messagesToReceive.Enqueue((messageCreateResult, messageCreatePayload.Bytes));

    SetupReceiveMessageSequence(mockWebSocket, messagesToReceive);

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(cts.Token);
    await Task.Delay(heartbeatInterval * 2);
    await cts.CancelAsync();

    readyHandler.Invocations.Should().HaveCount(0);
    firstCreateMessageHandler.Invocations.Should().HaveCount(0);
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