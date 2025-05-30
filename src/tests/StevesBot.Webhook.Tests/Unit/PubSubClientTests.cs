
namespace StevesBot.Webhook.Tests.Unit;

public sealed class PubSubClientTests : IDisposable
{
  private readonly MockHttpMessageHandler _mockHttpMessageHandler = new();
  private readonly Mock<ILogger<PubSubClient>> _mockLogger = new();
  private readonly PubSubClient _sut;

  public PubSubClientTests()
  {
    var mockHttpClient = _mockHttpMessageHandler.ToHttpClient();
    mockHttpClient.BaseAddress = new("https://test.com");

    _sut = new(mockHttpClient, _mockLogger.Object);
  }

  [Theory]
  [InlineData(HttpStatusCode.InternalServerError, false)]
  [InlineData(HttpStatusCode.OK, true)]
  public async Task SubscribeAsync_WhenCalled_ItShouldReturnExpectedResult(HttpStatusCode givenStatusCode, bool expectedResult)
  {
    _mockHttpMessageHandler
      .When("*/subscribe")
      .Respond(givenStatusCode);

    var result = await _sut.SubscribeAsync("callbackUrl", "topicUrl");

    result.Should().Be(expectedResult);
  }

  public void Dispose()
  {
    _mockHttpMessageHandler.Dispose();
  }
}