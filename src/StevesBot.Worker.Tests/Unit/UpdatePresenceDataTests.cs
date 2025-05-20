namespace StevesBot.Worker.Tests.Unit;

public class UpdatePresenceDataTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldCreateAnInstance()
  {
    var updatePresenceData = new UpdatePresenceData();

    updatePresenceData.Since.Should().BeNull();
    updatePresenceData.Activities.Should().BeEmpty();
    updatePresenceData.Status.Should().Be(PresenceStatus.Online);
    updatePresenceData.Afk.Should().BeFalse();
  }
  
  [Fact]
  public void Constructor_WhenCalledAndPropertiesAreInitialized_ItShouldCreateAnInstance()
  {
    var since = 1;
    var activities = new List<Activity>();
    var status = "some_made_up_status";
    var afk = true;
    
    var updatePresenceData = new UpdatePresenceData()
    {
      Since = since,
      Activities = activities,
      Status = status,
      Afk = afk,
    };
    
    updatePresenceData.Since.Should().Be(since);
    updatePresenceData.Activities.Should().BeSameAs(activities);
    updatePresenceData.Status.Should().Be(status);
    updatePresenceData.Afk.Should().Be(afk);
  }
}