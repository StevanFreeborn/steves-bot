using System.Text;

using WebSocket = StevesBot.Worker.WebSockets.WebSocket;

namespace StevesBot.Worker.Tests.Integration;

public sealed class WebSocketTests : IClassFixture<TestWebSocketServer>, IDisposable
{
  private readonly WebSocket _client = new();
  private readonly TestWebSocketServer _server;

  public WebSocketTests(TestWebSocketServer server)
  {
    _server = server;
  }

  [Fact]
  public async Task WebSocket_WhenUsed_ItShouldProperlyConnectSendMsgReceiveMsgAndClose()
  {
    await _client.ConnectAsync(_server.Uri, CancellationToken.None);

    var message = "Hello, WebSocket!";
    var buffer = new ArraySegment<byte>(Encoding.UTF8.GetBytes(message));
    await _client.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);

    var receivedBuffer = new ArraySegment<byte>(new byte[buffer.Count]);
    var receivedMessage = await _client.ReceiveAsync(receivedBuffer, CancellationToken.None);

    receivedMessage.MessageType.Should().Be(WebSocketMessageType.Text);
    receivedMessage.EndOfMessage.Should().BeTrue();
    receivedMessage.Count.Should().Be(buffer.Count);

    var receivedString = Encoding.UTF8.GetString([.. receivedBuffer], 0, receivedMessage.Count);
    receivedString.Should().Be(message);

    await _client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Test complete", CancellationToken.None);
  }

  public void Dispose()
  {
    _client.Dispose();
  }
}