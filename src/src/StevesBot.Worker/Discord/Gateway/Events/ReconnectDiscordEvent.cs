namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record ReconnectDiscordEvent : DiscordEvent
{
  public ReconnectDiscordEvent()
  {
    OpCode = DiscordOpCodes.Reconnect;
  }
}