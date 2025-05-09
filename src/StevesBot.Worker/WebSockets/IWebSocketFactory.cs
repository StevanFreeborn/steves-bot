namespace StevesBot.Worker.WebSockets;

internal interface IWebSocketFactory
{
  IWebSocket Create();
}