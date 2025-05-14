namespace StevesBot.Worker.Tests.Unit;

public sealed class DiscordGatewayClientTests : IDisposable
{
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<IWebSocketFactory> _mockWebSocketFactory = new();
  private readonly Mock<ILogger<DiscordGatewayClient>> _mockLogger = new();
  private readonly DiscordClientOptions _options = new();
# pragma warning disable CA2213
  private readonly DiscordGatewayClient _discordGatewayClient;

  public DiscordGatewayClientTests()
  {
    _discordGatewayClient = new DiscordGatewayClient(
      _options,
      _mockWebSocketFactory.Object,
      _mockLogger.Object,
      _mockDiscordRestClient.Object
    );
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

    var heatbeatInterval = 1000;

    var helloEvent = new
    {
      op = 10,
      d = new
      {
        heartbeat_interval = heatbeatInterval,
      }
    };

    var helloEventJson = JsonSerializer.Serialize(helloEvent);
    var helloEventBytes = Encoding.UTF8.GetBytes(helloEventJson);

    var heartbeatAck = new
    {
      op = 11,
    };

    var heartbeatAckJson = JsonSerializer.Serialize(heartbeatAck);
    var heartbeatAckBytes = Encoding.UTF8.GetBytes(heartbeatAckJson);

    messageQueue.Enqueue((new WebSocketReceiveResult(helloEventBytes.Length, WebSocketMessageType.Text, true), helloEventBytes));
    messageQueue.Enqueue((new WebSocketReceiveResult(heartbeatAckBytes.Length, WebSocketMessageType.Text, true), heartbeatAckBytes));

    mockWebSocket
      .Setup(static x => x.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
      .Returns(Task.CompletedTask);

    mockWebSocket
      .Setup(static x => x.ReceiveAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<CancellationToken>()))
      .Returns((ArraySegment<byte> buffer, CancellationToken token) =>
       {
         if (messageQueue.Count == 0)
         {
# pragma warning disable CA2008
           return Task.Delay(-1, token).ContinueWith(_ => new WebSocketReceiveResult(0, WebSocketMessageType.Text, true), token);
         }

         var (result, messageBytes) = messageQueue.Dequeue();
         Array.Copy(messageBytes, 0, buffer.Array!, buffer.Offset, Math.Min(messageBytes.Length, buffer.Count));
         return Task.FromResult(result);
       });

    _mockWebSocketFactory
      .Setup(static x => x.Create())
      .Returns(mockWebSocket.Object);

    using var cts = new CancellationTokenSource();

    await _discordGatewayClient.ConnectAsync(CancellationToken.None);

    await Task.Delay(heatbeatInterval + 30000);

    mockWebSocket.Verify(
      static x => x.SendAsync(It.IsAny<ArraySegment<byte>>(), It.IsAny<WebSocketMessageType>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()),
      Times.Once
    );
  }

  public void Dispose()
  {
  }
}