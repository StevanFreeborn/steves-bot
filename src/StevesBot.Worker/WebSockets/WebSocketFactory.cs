namespace StevesBot.Worker.WebSockets;

internal class WebSocketFactory : IWebSocketFactory
{
  public IWebSocket Create()
  {
    return new WebSocket();
  }
}