using System.Text.Json;

using Microsoft.Extensions.Options;

namespace StevesBot.Webhook.Tests.Unit;

public sealed class YouTubeDataApiClientTests : IDisposable
{
  private readonly MockHttpMessageHandler _mockHttpMessageHandler = new();
  private readonly Mock<ILogger<YouTubeDataApiClient>> _mockLogger = new();
  private readonly Mock<IOptions<YouTubeClientOptions>> _mockOptions = new();
  private readonly YouTubeDataApiClient _sut;

  public YouTubeDataApiClientTests()
  {
    var httpClient = _mockHttpMessageHandler.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://test.com");

    _mockOptions
      .Setup(static x => x.Value)
      .Returns(new YouTubeClientOptions());

    _sut = new(httpClient, _mockLogger.Object, _mockOptions.Object);
  }

  [Fact]
  public async Task GetVideoByIdAsync_WhenRequestFails_ItShouldReturnNull()
  {
    _mockHttpMessageHandler
      .When("*/videos")
      .Respond(HttpStatusCode.InternalServerError);

    var result = await _sut.GetVideoByIdAsync("video_id");

    result.Should().BeNull();
  }

  [Fact]
  public async Task GetVideoByIdAsync_WhenRequestSucceedsButItemsEmpty_ItShouldReturnNull()
  {
    var videosResponse = new YouTubeVideoListResponse();

    _mockHttpMessageHandler
      .When("*/videos")
      .Respond(
        HttpStatusCode.OK,
        "application/json",
        JsonSerializer.Serialize(videosResponse)
      );

    var result = await _sut.GetVideoByIdAsync("video_id", ["snippet"]);

    result.Should().BeNull();
  }

  [Fact]
  public async Task GetVideoByIdAsync_WhenRequestSucceedsAndVideoFound_ItShouldReturnVideo()
  {
    var videoId = "video_id";
    var video = new YouTubeVideo() { Id = videoId };
    var videosResponse = new YouTubeVideoListResponse() { Items = [video] };

    _mockHttpMessageHandler
      .When("*/videos")
      .Respond(
        HttpStatusCode.OK,
        "application/json",
        JsonSerializer.Serialize(videosResponse)
      );

    var result = await _sut.GetVideoByIdAsync(videoId);

    result.Should().BeEquivalentTo(video);

  }

  public void Dispose()
  {
    _mockHttpMessageHandler.Dispose();
  }
}