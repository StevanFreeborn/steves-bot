namespace StevesBot.Worker.Discord;

internal static class DiscordIntents
{
  public const long Guilds = 1 << 0;
  public const long GuildMembers = 1 << 1;
  public const long GuildModeration = 1 << 2;
  public const long GuildExpressions = 1 << 3;
  public const long GuildIntegrations = 1 << 4;
  public const long GuildWebhooks = 1 << 5;
  public const long GuildInvites = 1 << 6;
  public const long GuildVoiceStates = 1 << 7;
  public const long GuildPresences = 1 << 8;
  public const long GuildMessages = 1 << 9;
  public const long GuildMessageReactions = 1 << 10;
  public const long GuildMessageTyping = 1 << 11;
  public const long DirectMessages = 1 << 12;
  public const long DirectMessageReactions = 1 << 13;
  public const long DirectMessageTyping = 1 << 14;
  public const long MessageContent = 1 << 15;
  public const long GuildScheduledEvents = 1 << 16;
  public const long AutoModerationConfiguration = 1 << 20;
  public const long AutoModerationExecution = 1 << 21;
  public const long GuildMessagePolls = 1 << 24;
  public const long DirectMessagePolls = 1 << 25;
  public const long All = Guilds
    | GuildMembers
    | GuildModeration
    | GuildExpressions
    | GuildIntegrations
    | GuildWebhooks
    | GuildInvites
    | GuildVoiceStates
    | GuildPresences
    | GuildMessages
    | GuildMessageReactions
    | GuildMessageTyping
    | DirectMessages
    | DirectMessageReactions
    | DirectMessageTyping
    | MessageContent
    | GuildScheduledEvents
    | AutoModerationConfiguration
    | AutoModerationExecution
    | GuildMessagePolls
    | DirectMessagePolls;
}