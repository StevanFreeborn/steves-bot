using StevesBot.Library.Gemini;

namespace StevesBot.Worker.Handlers;

internal static class TaggedMessageHandler
{
  public static async Task HandleAsync(
    DiscordEvent discordEvent,
    IServiceProvider serviceProvider,
    CancellationToken cancellationToken = default
  )
  {
    var logger = serviceProvider.GetRequiredService<ILogger<IDiscordGatewayClient>>();
    var discordRestClient = serviceProvider.GetRequiredService<IDiscordRestClient>();
    var geminiClient = serviceProvider.GetRequiredService<IGeminiClient>();

    if (
      discordEvent is not MessageCreateDiscordEvent mcde ||
      mcde.IsMessageType(DiscordMessageTypes.Default) is false
    )
    {
      return;
    }

    var botUser = await discordRestClient.GetMeAsync(cancellationToken);

    if (mcde.Data.MentionsUser(botUser.Id) is false)
    {
      return;
    }

    logger.LogInformation("Bot tagged in message");

    var llmResponse = await geminiClient.GenerateContentAsync(mcde.Data.Content, cancellationToken);

    // TODO: LLM can be wordy...discord has 2000 character limit
    // on message size. Need to handle that.
    var request = new CreateMessageRequest(
      Content: llmResponse,
      MessageReference: new(
        Type: DiscordMessageReferenceTypes.Default,
        MessageId: mcde.Data.Id,
        ChannelId: mcde.Data.ChannelId,
        GuildId: mcde.Data.GuildId,
        FailIfNotExists: false
      )
    );

    var message = await discordRestClient.CreateMessageAsync(mcde.Data.ChannelId, request, cancellationToken);

    logger.LogInformation("Responded to tagged message with Id: {MessageId} for user: {UserId}", message.Id, mcde.Data.Author.Id);
  }
}