namespace StevesBot.Worker.Discord;

internal interface IDiscordRestClient
{
  Task<string> GetGatewayUrlAsync(CancellationToken cancellationToken);
}