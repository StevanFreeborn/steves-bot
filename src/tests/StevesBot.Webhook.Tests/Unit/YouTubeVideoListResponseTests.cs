namespace StevesBot.Webhook.Tests.Unit;

public class YouTubeVideoListResponseTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new YouTubeVideoListResponse();

    result.Items.Should().BeEmpty();
    result.PageInfo.Should().BeEquivalentTo(new YouTubePageInfo());
  }

  [Fact]
  public void Constructor_WhenCalledWithValues_ItShouldReturnAnInstance()
  {
    var videos = new YouTubeVideo[] { new() };
    var pageInfo = new YouTubePageInfo();

    var result = new YouTubeVideoListResponse()
    {
      Items = videos,
      PageInfo = pageInfo,
    };

    result.Items.Should().BeSameAs(videos);
    result.PageInfo.Should().BeSameAs(pageInfo);
  }
}