namespace StevesBot.Webhook.Tests.Unit;

public class YouTubeSnippetTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new YouTubeSnippet();

    result.LiveBroadcastContent.Should().BeEmpty();
  }

  [Fact]
  public void Constructor_WhenCalledWithValues_ItShouldReturnAnInstance()
  {
    var content = "live";

    var result = new YouTubeSnippet()
    {
      LiveBroadcastContent = content,
    };

    result.LiveBroadcastContent.Should().Be(content);
  }
}