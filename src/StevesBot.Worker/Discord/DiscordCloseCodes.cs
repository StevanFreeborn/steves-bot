namespace StevesBot.Worker.Discord;

internal static class DiscordCloseCodes
{
  private const int AuthenticationFailed = 4004;
  private const int InvalidShard = 4010;
  private const int ShardingRequired = 4011;
  private const int InvalidAPIVersion = 4012;
  private const int InvalidIntents = 4013;
  private const int DisallowedIntents = 4014;

  public static bool IsReconnectable(int? closeCode)
  {
    return closeCode is not AuthenticationFailed
      and not InvalidShard
      and not ShardingRequired
      and not InvalidAPIVersion
      and not InvalidIntents
      and not DisallowedIntents;
  }
}