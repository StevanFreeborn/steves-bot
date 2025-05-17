
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
    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, static (discordEvent, sp) =>
    {
      var discordRestClient = sp.GetRequiredService<IDiscordRestClient>();
      var logger = sp.GetRequiredService<ILogger<DiscordGatewayClient>>();

      // TODO: When a user join message is received,
      // we are going to reply to the message with
      // a welcome greeting
      // A user join message is type 8
      // Will need to use REST API to reply to the message

      if (
        discordEvent is MessageCreateDiscordEvent mcde &&
        mcde.IsMessageType(DiscordMessageTypes.UserJoin)
      )
      {
        logger.LogInformation("Guild Id: {GuildId}", mcde.Data.GuildId);
        logger.LogInformation("Channel Id: {ChannelId}", mcde.Data.ChannelId);
        logger.LogInformation("Message Id: {MessageId}", mcde.Data.Id);
        logger.LogInformation("User Id: {UserId}", mcde.Data.Author.Id);
        return Task.CompletedTask;
      }

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