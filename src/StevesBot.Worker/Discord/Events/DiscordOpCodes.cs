namespace StevesBot.Worker.Discord.Events;

internal static class DiscordOpCodes
{
  public const int Dispatch = 0;
  public const int Heartbeat = 1;
  public const int Identify = 2;
  public const int Resume = 6;
  public const int HeartbeatAck = 11;
  public const int Hello = 10;
}