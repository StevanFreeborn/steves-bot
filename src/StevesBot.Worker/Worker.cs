
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

    _discordGatewayClient.On(DiscordEventTypes.MessageCreate, static async (discordEvent, sp, cancellationToken) =>
    {
      var discordRestClient = sp.GetRequiredService<IDiscordRestClient>();
      var logger = sp.GetRequiredService<ILogger<DiscordGatewayClient>>();

      if (discordEvent is not MessageCreateDiscordEvent mcde || mcde.IsMessageType(DiscordMessageTypes.UserJoin) == false)
      {
        return;
      }

      logger.LogInformation("Received user join message for user: {UserId}", mcde.Data.Author.Id);

      var request = new CreateMessageRequest(
        Content: $"Welcome to the server <@{mcde.Data.Author.Id}>! We're glad to have you here.",
        MessageReference: new(
          Type: MessageReferenceTypes.Default,
          MessageId: mcde.Data.Id,
          ChannelId: mcde.Data.ChannelId,
          GuildId: mcde.Data.GuildId,
          FailIfNotExists: false
        )
      );

      var message = await discordRestClient.CreateMessageAsync(mcde.Data.ChannelId, request, cancellationToken);

      logger.LogInformation("Created welcome message with Id: {MessageId} for user: {UserId}", message.Id, mcde.Data.Author.Id);
    });

    return _discordGatewayClient.ConnectAsync(cancellationToken);
  }

  public Task StopAsync(CancellationToken cancellationToken)
  {
    _logger.LogInformation("Disconnecting Discord Gateway Client");
    return _discordGatewayClient.DisconnectAsync(cancellationToken);
  }
}