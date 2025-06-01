namespace StevesBot.Webhook.Tests.Unit;

public class YouTubeLiveStreamingDetailsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new YouTubeLiveStreamingDetails();

    result.ActualStartTime.Should().BeNull();
    result.ActualEndTime.Should().BeNull();
    result.ScheduledStartTime.Should().BeNull();
    result.ScheduledEndTime.Should().BeNull();
    result.ConcurrentViewers.Should().BeNull();
    result.ActiveLiveChatId.Should().BeNull();
  }

  [Fact]
  public void Constructor_WhenCalledWithValues_ItShouldReturnAnInstance()
  {
    var now = DateTimeOffset.UtcNow;
    var viewers = 100UL;
    var chatId = "chad_id";

    var result = new YouTubeLiveStreamingDetails()
    {
      ActualStartTime = now,
      ActualEndTime = now,
      ScheduledStartTime = now,
      ScheduledEndTime = now,
      ConcurrentViewers = viewers,
      ActiveLiveChatId = chatId,
    };

    result.ActualStartTime.Should().Be(now);
    result.ActualEndTime.Should().Be(now);
    result.ScheduledStartTime.Should().Be(now);
    result.ScheduledEndTime.Should().Be(now);
    result.ConcurrentViewers.Should().Be(viewers);
    result.ActiveLiveChatId.Should().Be(chatId);
  }
}