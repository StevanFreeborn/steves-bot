namespace StevesBot.Worker.Discord.Events;

internal record HeartbeatDiscordEvent : DiscordEvent
{
  public HeartbeatDiscordEvent(int? sequence)
  {
    OpCode = DiscordOpCodes.Heartbeat;
    Sequence = sequence;
  }
}