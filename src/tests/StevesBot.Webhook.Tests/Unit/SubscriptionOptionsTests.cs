namespace StevesBot.Webhook.Tests.Unit;

public class SubscriptionOptionsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var options = new SubscriptionOptions();

    options.CallbackUrl.Should().BeEmpty();
    options.TopicUrl.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithProperties_ItShouldReturnInstance()
  {
    var callbackUrl = "https://example.com/callback";
    var topicUrl = "https://example.com/topic";

    var options = new SubscriptionOptions
    {
      CallbackUrl = callbackUrl,
      TopicUrl = topicUrl
    };

    options.CallbackUrl.Should().Be(callbackUrl);
    options.TopicUrl.Should().Be(topicUrl);
  }
}