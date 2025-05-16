
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

    // TODO: Provide access to the gateway client
    // in delegate...don't try to resolve it from the service provider
    // TODO: Provide .On and .Off method overloads to allow
    // caller to not need to use discard for unused parameters
    _discordGatewayClient.On(DiscordEventTypes.Ready, static (discordEvent, sp) =>
    {
      var logger = sp.GetRequiredService<ILogger<DiscordGatewayClient>>();
      logger.LogInformation("Ready handler 1");
      return Task.CompletedTask;
    });

    _discordGatewayClient.On(DiscordEventTypes.Ready, static (discordEvent, sp) =>
    {
      var logger = sp.GetRequiredService<ILogger<DiscordGatewayClient>>();
      logger.LogInformation("Ready handler 2");
      return Task.CompletedTask;
    });

    return _discordGatewayClient.ConnectAsync(cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Disconnecting Discord Gateway Client");
    return _discordGatewayClient.DisconnectAsync(cancellationToken);
  }
}