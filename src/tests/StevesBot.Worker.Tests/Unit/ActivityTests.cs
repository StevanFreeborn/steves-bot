namespace StevesBot.Worker.Tests.Unit;

public class ActivityTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldCreateAnInstance()
  {
    var activity = new Activity();

    activity.Name.Should().Be(string.Empty);
    activity.Type.Should().Be(ActivityType.Custom);
    activity.State.Should().BeNull();
  }
}