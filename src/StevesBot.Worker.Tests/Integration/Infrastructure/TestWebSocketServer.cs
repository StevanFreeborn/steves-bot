using WebSocket = System.Net.WebSockets.WebSocket;

namespace StevesBot.Worker.Tests.Integration.Infrastructure;

# pragma warning disable CA1001

public sealed class TestWebSocketServer : IAsyncLifetime
{
  private readonly IWebHost _host;
  private readonly CancellationTokenSource _echoCts = new();
  private Uri? _webSocketUri;

  public Uri Uri
  {
    get
    {
      _webSocketUri ??= GetWebSocketUri();
      return _webSocketUri;
    }
  }

  public TestWebSocketServer()
  {
    _host = new WebHostBuilder()
      .Configure(app =>
      {
        app.UseWebSockets();

        app.Run(async context =>
        {
          if (context.Request.Path == "/ws" && context.WebSockets.IsWebSocketRequest)
          {
            var webSocket = await context.WebSockets.AcceptWebSocketAsync();
            await Echo(webSocket, _echoCts.Token);
          }
          else
          {
            context.Response.StatusCode = 400;
          }
        });
      })
      .UseKestrel()
      .UseUrls("http://[::1]:0")
      .Build();
  }

  public async Task InitializeAsync()
  {
    await _host.StartAsync();
  }

  public async Task DisposeAsync()
  {
    await _echoCts.CancelAsync();
    await _host.StopAsync();

    _echoCts.Dispose();
    _host.Dispose();
  }

  private Uri GetWebSocketUri()
  {
    var address = _host.ServerFeatures.GetRequiredFeature<IServerAddressesFeature>().Addresses.First();
    var webSocketAddress = address.Replace("http://", "ws://", StringComparison.OrdinalIgnoreCase);
    return new Uri(webSocketAddress + "/ws");
  }

  private static async Task Echo(WebSocket webSocket, CancellationToken cancellationToken)
  {
    var buffer = new byte[1024 * 4];

    try
    {
      var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);

      while (result.CloseStatus.HasValue is false && cancellationToken.IsCancellationRequested is false)
      {
        await webSocket.SendAsync(new ArraySegment<byte>(buffer, 0, result.Count), result.MessageType, result.EndOfMessage, cancellationToken);
        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), cancellationToken);
      }

      if (result.CloseStatus.HasValue && cancellationToken.IsCancellationRequested is false)
      {
        await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
      }
    }
    catch (WebSocketException ex) when (ex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely)
    {
      Console.WriteLine($"{nameof(TestWebSocketServer)}: Client connection closed prematurely.");
    }
    catch (OperationCanceledException)
    {
      Console.WriteLine($"{nameof(TestWebSocketServer)}: Echo operation cancelled (server shutting down).");

      if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
      {
        await webSocket.CloseAsync(WebSocketCloseStatus.EndpointUnavailable, "Server shutting down", CancellationToken.None);
      }
    }
    catch (Exception ex)
    {
      Console.WriteLine($"{nameof(TestWebSocketServer)}: Error during echo: {ex.Message}");

      if (webSocket.State is WebSocketState.Open or WebSocketState.CloseReceived)
      {
        await webSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server error", CancellationToken.None);
      }
    }
    finally
    {
      webSocket.Dispose();
    }
  }
}