namespace StevesBot.Worker.Tests.Unit;

public class UpdatePresenceDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var activities = new List<Activity>();
    var since = 1234567890L;

    var e = new UpdatePresenceDiscordEvent(since, activities, PresenceStatus.Online, false);

    e.Should().NotBeNull();
    e.Data.Should().NotBeNull();
    e.Data.Since.Should().Be(since);
    e.Data.Activities.Should().BeSameAs(activities);
    e.Data.Status.Should().Be(PresenceStatus.Online);
    e.Data.Afk.Should().BeFalse();
  }
}