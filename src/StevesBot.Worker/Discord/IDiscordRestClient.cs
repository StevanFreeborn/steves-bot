namespace StevesBot.Worker.Discord;

internal interface IDiscordRestClient
{
  Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken);
  Task<MessageCreateData> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken);
}