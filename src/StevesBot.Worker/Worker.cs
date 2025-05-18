
namespace StevesBot.Worker;

internal class Worker : IHostedService
{
  private readonly ILogger<Worker> _logger;
  private readonly IDiscordGatewayClient _discordGatewayClient;

  public Worker(ILogger<Worker> logger, IDiscordGatewayClient discordGatewayClient)
  {
    _logger = logger;
    _discordGatewayClient = discordGatewayClient;
  }

  public Task StartAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Connecting Discord Gateway Client");
    return _discordGatewayClient.ConnectAsync(cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Disconnecting Discord Gateway Client");
    return _discordGatewayClient.DisconnectAsync(cancellationToken);
  }
}