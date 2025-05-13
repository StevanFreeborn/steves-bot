namespace StevesBot.Worker.Discord;

internal interface IDiscordGatewayClient : IDisposable
{
  Task ConnectAsync(CancellationToken cancellationToken);
}