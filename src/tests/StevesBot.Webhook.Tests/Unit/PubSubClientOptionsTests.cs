namespace StevesBot.Webhook.Tests.Unit;

public class PubSubClientOptionsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var options = new PubSubClientOptions();

    options.BaseUrl.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithProperties_ItShouldReturnInstance()
  {
    var baseUrl = "https://example.com";

    var options = new PubSubClientOptions()
    {
      BaseUrl = baseUrl
    };

    options.BaseUrl.Should().Be(baseUrl);
  }
}