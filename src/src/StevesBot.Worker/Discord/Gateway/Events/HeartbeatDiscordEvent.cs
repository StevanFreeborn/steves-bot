namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record HeartbeatDiscordEvent : DiscordEvent
{
  public HeartbeatDiscordEvent(int? sequence)
  {
    OpCode = DiscordOpCodes.Heartbeat;
    Sequence = sequence;
  }
}