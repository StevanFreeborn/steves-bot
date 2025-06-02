using System.Globalization;
using System.Text;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

using StevesBot.Library.Discord.Rest;
using StevesBot.Library.Discord.Rest.Requests;
using StevesBot.Webhook.YouTube.Handlers;

namespace StevesBot.Webhook.Tests.Unit;

public class NotificationHandlerTests
{
  private readonly Mock<HttpRequest> _mockHttpRequest = new();
  private readonly Mock<HttpContext> _mockHttpContext = new();
  private readonly Mock<ILogger<Program>> _mockLogger = new();
  private readonly Mock<IYouTubeDataApiClient> _mockYtDataApiClient = new();
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<IOptionsMonitor<DiscordNotificationOptions>> _mockNotificationsOptions = new();
  private readonly Mock<ILastPostedStreamStore> _mockLastPostedStore = new();

  public NotificationHandlerTests()
  {
    _mockHttpContext
      .Setup(static x => x.Request)
      .Returns(_mockHttpRequest.Object);
  }

  [Fact]
  public async Task HandleAsync_WhenVideoIdNotFound_ItShouldReturnBadRequest()
  {
    SetupMockRequestBodyStream("This is a test");

    var result = await HandleAsync();

    result.Should().BeOfType<BadRequest<string>>();
  }

  [Fact]
  public async Task HandleAsync_WhenVideoIsNotFound_ItShouldReturnNotFound()
  {
    SetupMockRequestBodyStream("<yt:videoId>videoId</yt:videoId>");

    _mockYtDataApiClient
      .Setup(static x => x.GetVideoByIdAsync(
        It.IsAny<string>(),
        It.IsAny<string[]>(),
        It.IsAny<CancellationToken>()
      ))
      .ReturnsAsync(null as YouTubeVideo);

    var result = await HandleAsync();

    result.Should().BeOfType<NotFound>();
  }

  [Fact]
  public async Task HandleAsync_WhenVideoIsFoundButIsNotALiveStream_ItShouldReturnOkButNotCreateDiscordMessage()
  {
    SetupMockRequestBodyStream("<yt:videoId>videoId</yt:videoId>");

    _mockYtDataApiClient
      .Setup(static x => x.GetVideoByIdAsync(
        It.IsAny<string>(),
        It.IsAny<string[]>(),
        It.IsAny<CancellationToken>()
      ))
      .ReturnsAsync(new YouTubeVideo());

    var result = await HandleAsync();

    result.Should().BeOfType<Ok>();

    _mockLastPostedStore
      .Verify(static x => x.SetValue(It.IsAny<string>()), Times.Never);

    _mockDiscordRestClient
      .Verify(
        static x => x.CreateMessageAsync(
          It.IsAny<string>(),
          It.IsAny<CreateMessageRequest>(),
          It.IsAny<CancellationToken>()
        ),
        Times.Never
      );
  }

  [Fact]
  public async Task HandleAsync_WhenVideoIsFoundButItHasSameIdAsLastVideo_ItShouldReturnOkButNotCreateDiscordMessage()
  {
    var videoId = "video_id";

    SetupMockRequestBodyStream($"<yt:videoId>{videoId}</yt:videoId>");

    _mockYtDataApiClient
      .Setup(static x => x.GetVideoByIdAsync(
        It.IsAny<string>(),
        It.IsAny<string[]>(),
        It.IsAny<CancellationToken>()
      ))
      .ReturnsAsync(new YouTubeVideo()
      {
        Id = videoId,
        LiveStreamingDetails = new(),
        Snippet = new()
        {
          LiveBroadcastContent = "live"
        }
      });

    _mockLastPostedStore
      .Setup(static x => x.HasValue(It.IsAny<string>()))
      .Returns(true);

    var result = await HandleAsync();

    result.Should().BeOfType<Ok>();

    _mockLastPostedStore
      .Verify(static x => x.SetValue(It.IsAny<string>()), Times.Never);

    _mockDiscordRestClient
      .Verify(
        static x => x.CreateMessageAsync(
          It.IsAny<string>(),
          It.IsAny<CreateMessageRequest>(),
          It.IsAny<CancellationToken>()
        ),
        Times.Never
      );
  }

  [Fact]
  public async Task HandleAsync_WhenVideoIsFoundAndItIsANewLiveStream_ItShouldReturnOkStoreTheIdAndCreateADiscordMessage()
  {
    var videoId = "video_id";

    SetupMockRequestBodyStream($"<yt:videoId>{videoId}</yt:videoId>");

    _mockYtDataApiClient
      .Setup(static x => x.GetVideoByIdAsync(
        It.IsAny<string>(),
        It.IsAny<string[]>(),
        It.IsAny<CancellationToken>()
      ))
      .ReturnsAsync(new YouTubeVideo()
      {
        Id = videoId,
        LiveStreamingDetails = new(),
        Snippet = new()
        {
          LiveBroadcastContent = "live"
        }
      });

    _mockLastPostedStore
      .Setup(static x => x.HasValue(It.IsAny<string>()))
      .Returns(false);

    _mockNotificationsOptions
      .Setup(x => x.CurrentValue)
      .Returns(new DiscordNotificationOptions());

    var result = await HandleAsync();

    result.Should().BeOfType<Ok>();

    _mockLastPostedStore
      .Verify(
        x => x.SetValue(
          It.Is<string>(s => string.Equals(s, videoId, StringComparison.Ordinal))
        ),
        Times.Once
      );

    _mockDiscordRestClient
      .Verify(
        static x => x.CreateMessageAsync(
          It.IsAny<string>(),
          It.IsAny<CreateMessageRequest>(),
          It.IsAny<CancellationToken>()
        ),
        Times.Once
      );
  }

  private void SetupMockRequestBodyStream(string content)
  {
    var stream = new MemoryStream(Encoding.UTF8.GetBytes(content));
    _mockHttpRequest.Setup(static x => x.Body).Returns(stream);
  }

  private Task<IResult> HandleAsync()
  {
    return NotificationHandler.HandleAsync(
      _mockHttpContext.Object,
      _mockLogger.Object,
      _mockYtDataApiClient.Object,
      _mockDiscordRestClient.Object,
      _mockNotificationsOptions.Object,
      _mockLastPostedStore.Object
    );
  }
}