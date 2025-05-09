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
  };
}