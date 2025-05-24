using Activity = StevesBot.Worker.Discord.Gateway.Events.Data.Activity;

namespace StevesBot.Worker.Discord.Gateway.Events;

internal sealed record UpdatePresenceDiscordEvent : DiscordEvent
{
  [JsonPropertyName("d")]
  public new UpdatePresenceData Data { get; init; } = new UpdatePresenceData();

  public UpdatePresenceDiscordEvent(long? since, List<Activity> activities, string status, bool afk)
  {
    OpCode = DiscordOpCodes.PresenceUpdate;
    Data = new UpdatePresenceData
    {
      Since = since,
      Activities = activities,
      Status = status,
      Afk = afk,
    };
  }
}
