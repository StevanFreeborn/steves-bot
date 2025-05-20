namespace StevesBot.Worker.Tests.Unit;

public class ActivityTypeTests
{
  [Theory]
  [MemberData(nameof(TestData))]
  public void ActivityType_WhenCalled_ItShouldReturnExpectedResult(int activityType, int expected)
  {
    activityType.Should().Be(expected);
  }

  public static TheoryData<int, int> TestData => new()
  {
    {
      ActivityType.Custom,
      4
    },
  };
}