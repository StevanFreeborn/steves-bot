using StevesBot.Library.Discord.Common;
using StevesBot.Library.Discord.Rest.Requests;

namespace StevesBot.Library.Discord.Rest;

public interface IDiscordRestClient
{
  Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken = default);
  Task<DiscordMessage> CreateMessageAsync(string channelId, CreateMessageRequest request, CancellationToken cancellationToken = default);
}