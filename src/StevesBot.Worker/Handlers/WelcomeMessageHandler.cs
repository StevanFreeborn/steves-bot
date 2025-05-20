using System.Globalization;
namespace StevesBot.Worker.Handlers;

internal static class WelcomeMessageHandler
{
  public static async Task HandleAsync(
    DiscordEvent discordEvent,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken
  )
  {
    var discordRestClient = serviceProvider.GetRequiredService<IDiscordRestClient>();
    var logger = serviceProvider.GetRequiredService<ILogger<DiscordGatewayClient>>();

    if (discordEvent is not MessageCreateDiscordEvent mcde || mcde.IsMessageType(DiscordMessageTypes.UserJoin) == false)
    {
      return;
    }

    logger.LogInformation("Received user join message for user: {UserId}", mcde.Data.Author.Id);
    var welcomeMessage = GetWelcomeMessage(mcde.Data.Author.Id);

    var request = new CreateMessageRequest(
      Content: welcomeMessage,
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
  }

  private static string GetWelcomeMessage(string userId)
  {
    var builder = new StringBuilder();

    builder.AppendLine(CultureInfo.InvariantCulture, $"Hi there <@{userId}>! 👋🏻 Welcome to Stevan's server.");
    builder.AppendLine();
    builder.AppendLine("We're so stoked to have you join our community here. Feel free to jump right in, tell us a bit about yourself, and explore all the different channels.");
    builder.AppendLine();
    builder.AppendLine("If you have any questions at all, don't hesitate to ask. We're a friendly bunch and always happy to help out. Glad you're here!");

    return builder.ToString();
  }
}