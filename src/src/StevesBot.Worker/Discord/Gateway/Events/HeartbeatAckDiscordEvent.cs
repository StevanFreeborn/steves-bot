namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record HeartbeatAckDiscordEvent : DiscordEvent
{
  public HeartbeatAckDiscordEvent()
  {
    OpCode = DiscordOpCodes.HeartbeatAck;
  }
}