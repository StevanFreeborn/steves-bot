namespace StevesBot.Worker.Discord;

internal class DiscordGatewayClient : IDiscordGatewayClient
{
  private readonly DiscordGatewayClientOptions _options;
  private readonly IWebSocketFactory _webSocketFactory;
  private readonly ILogger<DiscordGatewayClient> _logger;
  private readonly JsonSerializerOptions _jsonSerializerOptions = new()
  {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    ReferenceHandler = ReferenceHandler.IgnoreCycles,
    Converters =
    {
      new DiscordEventConverter(),
    },
  };
  private readonly AsyncLock _lock = new();

  private string _gatewayUrl = string.Empty;
  private IWebSocket? _webSocket;
  private Task? _receiveTask;
  private DateTime _timeLastHeartbeatSent = DateTime.MinValue;
  private DateTime _timeLastHeartbeatAcknowledged = DateTime.MinValue;
  private CancellationTokenSource? _heartbeatCts;
  private Task? _heartbeatTask;
  private string _sessionId = string.Empty;
  private string _resumeGatewayUrl = string.Empty;

  public DiscordGatewayClient(
    DiscordGatewayClientOptions options,
    IWebSocketFactory webSocketFactory,
    ILogger<DiscordGatewayClient> logger
  )
  {
    _options = options ?? throw new ArgumentNullException(nameof(options));
    _webSocketFactory = webSocketFactory ?? throw new ArgumentNullException(nameof(webSocketFactory));
    _logger = logger ?? throw new ArgumentNullException(nameof(logger));
  }

  public void Dispose()
  {
    _heartbeatCts?.Cancel();
    _heartbeatCts?.Dispose();
    _heartbeatTask?.Dispose();
    _receiveTask?.Dispose();
    _webSocket?.Dispose();
  }
}