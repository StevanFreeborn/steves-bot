namespace StevesBot.Worker.Tests.Unit;

public class PresenceStatusTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void PresenceStatus_WhenCalled_ItShouldReturnExpectedResult(string status, string expected)
  {
    status.Should().Be(expected);
  }

  public static TheoryData<string, string> TestData => new()
  {
    {
      PresenceStatus.Online,
      "online"
    },
    {
      PresenceStatus.Idle,
      "idle"
    },
  };
}