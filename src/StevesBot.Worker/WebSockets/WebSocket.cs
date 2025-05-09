namespace StevesBot.Worker.WebSockets;

internal class WebSocket : IWebSocket
{
  private readonly ClientWebSocket _clientWebSocket;

  public WebSocketState State => _clientWebSocket.State;

  public WebSocket(ClientWebSocket? clientWebSocket = null)
  {
    _clientWebSocket = clientWebSocket ?? new ClientWebSocket();
  }

  public async Task ConnectAsync(Uri uri, CancellationToken cancellationToken)
  {
    await _clientWebSocket.ConnectAsync(uri, cancellationToken);
  }

  public async Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
  {
    await _clientWebSocket.CloseAsync(closeStatus, statusDescription, cancellationToken);
  }

  public async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
  {
    return await _clientWebSocket.ReceiveAsync(buffer, cancellationToken);
  }

  public async Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
  {
    await _clientWebSocket.SendAsync(buffer, messageType, endOfMessage, cancellationToken);
  }

  public void Dispose()
  {
    _clientWebSocket.Dispose();
  }
}