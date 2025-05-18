namespace StevesBot.Worker.Discord.Rest;

internal interface IDiscordRestClient
{
  Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken);
  Task<DiscordMessage> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken);
}