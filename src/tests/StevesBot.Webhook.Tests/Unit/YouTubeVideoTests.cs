namespace StevesBot.Webhook.Tests.Unit;

public class YouTubeVideoTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new YouTubeVideo();

    result.Id.Should().BeEmpty();
    result.LiveStreamingDetails.Should().BeNull();
    result.Snippet.Should().BeEquivalentTo(new YouTubeSnippet());
  }

  [Fact]
  public void Constructor_WhenCalledWithValues_ItShouldReturnAnInstance()
  {
    var id = "id";
    var details = new YouTubeLiveStreamingDetails();
    var snippet = new YouTubeSnippet();

    var result = new YouTubeVideo()
    {
      Id = id,
      LiveStreamingDetails = details,
      Snippet = snippet,
    };

    result.Id.Should().Be(id);
    result.LiveStreamingDetails.Should().BeSameAs(details);
    result.Snippet.Should().BeSameAs(snippet);
  }

  [Fact]
  public void IsLiveStream_WhenLiveStreamingDetailsIsNull_ItShouldReturnFalse()
  {
    var video = new YouTubeVideo()
    {
      Snippet = new() { LiveBroadcastContent = "live" },
    };

    video.IsLiveStream.Should().BeFalse();
  }

  [Fact]
  public void IsLiveStream_WhenSnippetLiveBroadcastContentIsNotLive_ItShouldReturnFalse()
  {
    var video = new YouTubeVideo()
    {
      LiveStreamingDetails = new(),
      Snippet = new() { LiveBroadcastContent = "none" },
    };

    video.IsLiveStream.Should().BeFalse();
  }

  [Fact]
  public void IsLiveStream_WhenDetailsIsNotNullAndContentIsLive_ItShouldReturnTrue()
  {
    var video = new YouTubeVideo()
    {
      LiveStreamingDetails = new(),
      Snippet = new() { LiveBroadcastContent = "live" },
    };

    video.IsLiveStream.Should().BeTrue();
  }
}