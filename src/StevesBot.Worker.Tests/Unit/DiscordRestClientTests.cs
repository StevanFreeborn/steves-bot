namespace StevesBot.Worker.Tests.Unit;

public sealed class DiscordRestClientTests : IDisposable
{
  private const string BaseUrl = "https://discord.com/api/v10";
  private static string GatewayEndpoint => $"{BaseUrl}/gateway";

  private readonly Mock<ILogger<DiscordRestClient>> _loggerMock = new();
  private readonly MockHttpMessageHandler _mockHttpMessageHandler;
  private readonly DiscordRestClient _discordRestClient;

  public DiscordRestClientTests()
  {
    _mockHttpMessageHandler = new MockHttpMessageHandler();
    var httpClient = _mockHttpMessageHandler.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://discord.com/api/v10/");
    _discordRestClient = new DiscordRestClient(_loggerMock.Object, httpClient);
  }

  [Fact]
  public async Task GetGatewayUrlAsync_WhenRequestFails_ItShouldThrowException()
  {
    _mockHttpMessageHandler
      .When(GatewayEndpoint)
      .Respond(HttpStatusCode.InternalServerError);

    var act = async () => await _discordRestClient.GetGatewayUrlAsync(CancellationToken.None);

    await act.Should().ThrowAsync<DiscordRestClientException>();
  }

  [Fact]
  public async Task GetGatewayUrlAsync_WhenResponseIsNull_ItShouldThrowException()
  {
    _mockHttpMessageHandler
      .When(GatewayEndpoint)
      .Respond(HttpStatusCode.OK, "application/json", "null");

    var act = async () => await _discordRestClient.GetGatewayUrlAsync(CancellationToken.None);

    await act.Should().ThrowAsync<DiscordRestClientException>();
  }

  [Fact]
  public async Task GetGatewayUrlAsync_WhenRequestIsSuccessful_ItShouldReturnGatewayUrl()
  {
    var expectedUrl = "test";
    var jsonResponse = $@"{{ 
      ""url"": ""{expectedUrl}"" 
    }}";

    _mockHttpMessageHandler
      .When(GatewayEndpoint)
      .Respond(HttpStatusCode.OK, "application/json", jsonResponse);

    var result = await _discordRestClient.GetGatewayUrlAsync(CancellationToken.None);

    result.Should().Be(expectedUrl);
  }

  public void Dispose()
  {
    _mockHttpMessageHandler.Dispose();
  }
}