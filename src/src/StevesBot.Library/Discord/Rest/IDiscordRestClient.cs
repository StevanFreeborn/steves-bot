using StevesBot.Library.Discord.Common;
using StevesBot.Library.Discord.Rest.Requests;

namespace StevesBot.Library.Discord.Rest;

public interface IDiscordRestClient
{
  Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken = default);
  Task<DiscordMessage> CreateMessageAsync(
    string channelId,
    CreateMessageRequest request,
    CancellationToken cancellationToken = default
  );
  Task<DiscordChannel> CreateThreadFromMessageAsync(
    string channelId,
    string messageId,
    CreateThreadFromMessageRequest request,
    CancellationToken cancellationToken = default
  );
  Task<DiscordUser> GetMeAsync(CancellationToken cancellationToken = default);
  Task<DiscordChannel> GetChannelAsync(
    string channelId,
    CancellationToken cancellationToken = default
  );
  Task StartTypingAsync(string channelId, CancellationToken cancellationToken = default);
}