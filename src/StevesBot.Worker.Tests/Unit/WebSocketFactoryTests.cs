using WebSocket = StevesBot.Worker.WebSockets.WebSocket;

namespace StevesBot.Worker.Tests.Unit;

public class WebSocketFactoryTests
{
  private readonly WebSocketFactory _webSocketFactory = new();

  [Fact]
  public void Create_WhenCalled_ItShouldReturnNewWebSocketInstance()
  {
    var result = _webSocketFactory.Create();

    result.Should().NotBeNull();
    result.Should().BeOfType<WebSocket>();
  }
}