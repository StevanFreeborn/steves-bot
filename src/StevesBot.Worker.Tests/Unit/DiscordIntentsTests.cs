namespace StevesBot.Worker.Tests.Unit;

public class DiscordIntentsTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void Intents_ShouldHaveCorrectValue(long intent, long expectedValue)
  {
    intent.Should().Be(expectedValue);
  }

  public static TheoryData<long, long> TestData => new()
  {
    { DiscordIntents.Guilds, 1 },
    { DiscordIntents.GuildMembers, 2 },
    { DiscordIntents.GuildModeration, 4 },
    { DiscordIntents.GuildExpressions, 8 },
    { DiscordIntents.GuildIntegrations, 16 },
    { DiscordIntents.GuildWebhooks, 32 },
    { DiscordIntents.GuildInvites, 64 },
    { DiscordIntents.GuildVoiceStates, 128 },
    { DiscordIntents.GuildPresences, 256 },
    { DiscordIntents.GuildMessages, 512 },
    { DiscordIntents.GuildMessageReactions, 1024 },
    { DiscordIntents.GuildMessageTyping, 2048 },
    { DiscordIntents.DirectMessages, 4096 },
    { DiscordIntents.DirectMessageReactions, 8192 },
    { DiscordIntents.DirectMessageTyping, 16384 },
    { DiscordIntents.MessageContent, 32768 },
    { DiscordIntents.GuildScheduledEvents, 65536 },
    { DiscordIntents.AutoModerationConfiguration, 1048576 },
    { DiscordIntents.AutoModerationExecution, 2097152 },
    { DiscordIntents.GuildMessagePolls, 16777216 },
    { DiscordIntents.DirectMessagePolls, 33554432 },
    { DiscordIntents.All, 53608447 }
  };
}