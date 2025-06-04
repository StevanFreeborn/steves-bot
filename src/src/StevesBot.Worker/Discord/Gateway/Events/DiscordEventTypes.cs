namespace StevesBot.Worker.Discord.Gateway.Events;

internal static class DiscordEventTypes
{
  public const string Ready = "READY";
  public const string GuildMemberAdd = "GUILD_MEMBER_ADD";
  public const string MessageCreate = "MESSAGE_CREATE";

  public static bool IsValidEvent(string eventName)
  {
    if (string.IsNullOrWhiteSpace(eventName))
    {
      return false;
    }

    return eventName is Ready or
      GuildMemberAdd or
      MessageCreate;
  }
}