namespace StevesBot.Worker.Tests.Unit;

public class DiscordEventTypesTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void Event_WhenCalled_ItShouldReturnCorrectValue(string eventType, string expectedValue)
  {
    eventType.Should().Be(expectedValue);
  }

  public static TheoryData<string, string> TestData => new()
  {
    { DiscordEventTypes.Ready, "READY" },
    { DiscordEventTypes.GuildMemberAdd, "GUILD_MEMBER_ADD" },
    { DiscordEventTypes.MessageCreate, "MESSAGE_CREATE" },
  };

  [Theory]
  [MemberData(nameof(IsValidEventNameTestData))]
  public void IsValidEventName_WhenCalled_ItShouldReturnCorrectValue(string? eventName, bool expected)
  {
    var result = DiscordEventTypes.IsValidEvent(eventName!);

    result.Should().Be(expected);
  }

  public static TheoryData<string?, bool> IsValidEventNameTestData => new()
  {
    { "  ", false },
    { "", false },
    { null, false },
    { "I MADE IT UP", false },
    { DiscordEventTypes.Ready, true },
    { DiscordEventTypes.GuildMemberAdd, true },
    { DiscordEventTypes.MessageCreate, true },
  };
}