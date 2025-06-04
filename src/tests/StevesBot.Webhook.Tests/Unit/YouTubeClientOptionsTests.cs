namespace StevesBot.Webhook.Tests.Unit;

public class YouTubeClientOptionsTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var options = new YouTubeClientOptions();

    options.BaseUrl.Should().BeEmpty();
    options.ApiKey.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithProperties_ItShouldReturnInstance()
  {
    var baseUrl = "https://example.com";
    var apiKey = "test-api-key";

    var options = new YouTubeClientOptions
    {
      BaseUrl = baseUrl,
      ApiKey = apiKey
    };

    options.BaseUrl.Should().Be(baseUrl);
    options.ApiKey.Should().Be(apiKey);
  }
}