namespace StevesBot.Worker.Discord;

internal interface IDiscordGatewayClient : IDisposable
{
  Task ConnectAsync(CancellationToken cancellationToken);
  Task DisconnectAsync(CancellationToken cancellationToken);
  void On(string eventName, Func<DiscordEvent, IServiceProvider, CancellationToken, Task> handler);
  void Off(string eventName, Func<DiscordEvent, IServiceProvider, CancellationToken, Task> handler);
}