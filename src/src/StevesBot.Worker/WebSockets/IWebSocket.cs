namespace StevesBot.Worker.WebSockets;

internal interface IWebSocket : IDisposable
{
  WebSocketState State { get; }

  Task ConnectAsync(Uri uri, CancellationToken cancellationToken);
  Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken);
  Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken);
  Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken);
}