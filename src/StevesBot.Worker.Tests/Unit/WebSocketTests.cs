using WebSocket = StevesBot.Worker.WebSockets.WebSocket;

namespace StevesBot.Worker.Tests.Unit;

public class WebSocketTests
{
  [Fact]
  public void State_WhenCalled_ItShouldReturnWebSocketState()
  {
    using var webSocket = new WebSocket();
    var state = webSocket.State;

    state.Should().Be(WebSocketState.None);
  }
}