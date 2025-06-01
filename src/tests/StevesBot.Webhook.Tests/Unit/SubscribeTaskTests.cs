namespace StevesBot.Webhook.Tests.Unit;

public class SubscribeTaskTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var task = new SubscribeTask();

    task.CallbackUrl.Should().BeEmpty();
    task.TopicUrl.Should().BeEmpty();
    task.ExpiresAt.Should().Be(DateTimeOffset.MinValue);
  }

  [Fact]
  public void Constructor_WhenCalledWithProperties_ItShouldReturnInstance()
  {
    var callbackUrl = "https://example.com/callback";
    var topicUrl = "https://example.com/topic";
    var expiresAt = DateTime.UtcNow.AddDays(1);

    var task = new SubscribeTask
    {
      CallbackUrl = callbackUrl,
      TopicUrl = topicUrl,
      ExpiresAt = expiresAt,
    };

    task.CallbackUrl.Should().Be(callbackUrl);
    task.TopicUrl.Should().Be(topicUrl);
    task.ExpiresAt.Should().Be(expiresAt);
  }
}