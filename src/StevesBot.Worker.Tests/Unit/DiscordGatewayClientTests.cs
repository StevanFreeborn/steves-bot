namespace StevesBot.Worker.Tests.Unit;

public sealed class DiscordGatewayClientTests : IDisposable
{
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<IWebSocketFactory> _mockWebSocketFactory = new();
  private readonly Mock<ILogger<DiscordGatewayClient>> _mockLogger = new();
  private readonly DiscordClientOptions _options = new();
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

  public void Dispose()
  {
    _discordGatewayClient.Dispose();
  }
}