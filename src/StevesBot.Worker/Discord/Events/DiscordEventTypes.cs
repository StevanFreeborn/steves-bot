namespace StevesBot.Worker.Discord.Events;

internal static class DiscordEventTypes
{
  public const string Ready = "READY";

  public static bool IsValidEvent(string eventName)
  {
    if (string.IsNullOrEmpty(eventName))
    {
      return false;
    }

    return eventName is Ready;
  }
}