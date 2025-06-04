namespace StevesBot.Library.Tests.Unit;

public sealed class DiscordRestClientTests : IDisposable
{
  private const string BaseUrl = "https://discord.com/api/v10";
  private static string GatewayEndpoint => $"{BaseUrl}/gateway";
  private static string ChannelMessagesEndpoint => $"{BaseUrl}/channels/*/messages";

  private readonly Mock<ILogger<DiscordRestClient>> _mockLogger = new();
  private readonly MockHttpMessageHandler _mockHttpMessageHandler;
  private readonly DiscordRestClient _discordRestClient;

  public DiscordRestClientTests()
  {
    _mockHttpMessageHandler = new MockHttpMessageHandler();
    var httpClient = _mockHttpMessageHandler.ToHttpClient();
    httpClient.BaseAddress = new Uri("https://discord.com/api/v10/");
    _discordRestClient = new DiscordRestClient(_mockLogger.Object, httpClient);
  }

  [Fact]
  public void Constructor_WhenLoggerIsNull_ItShouldThrowAnException()
  {
    var act = () => new DiscordRestClient(null!, _mockHttpMessageHandler.ToHttpClient());

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void Constructor_WhenHttpClientIsNull_ItShouldThrowAnException()
  {
    var act = () => new DiscordRestClient(_mockLogger.Object, null!);

    act.Should().Throw<ArgumentNullException>();
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

  [Fact]
  public async Task CreateMessageAsync_WhenRequestFails_ItShouldThrowAnException()
  {
    _mockHttpMessageHandler
      .When(ChannelMessagesEndpoint)
      .Respond(HttpStatusCode.InternalServerError);

    var request = new CreateMessageRequest(
      "content",
      new(DiscordMessageReferenceTypes.Default, "message_id", "channel_id", "guild_id", false)
    );

    var act = async () => await _discordRestClient.CreateMessageAsync("channel_id", request);

    await act.Should().ThrowAsync<DiscordRestClientException>();
  }

  [Fact]
  public async Task CreateMessageyAsync_WhenResponseIsNull_ItShouldThrowAnException()
  {
    _mockHttpMessageHandler
      .When(ChannelMessagesEndpoint)
      .Respond(HttpStatusCode.OK, "application/json", "null");

    var request = new CreateMessageRequest(
      "content",
      new(DiscordMessageReferenceTypes.Default, "message_id", "channel_id", "guild_id", false)
    );

    var act = async () => await _discordRestClient.CreateMessageAsync("channel_id", request);

    await act.Should().ThrowAsync<DiscordRestClientException>();
  }

  [Fact]
  public async Task CreateMessageAsync_WhenRequestSucceeds_ItShouldReturnMessage()
  {
    var message = new DiscordMessage();
    var messageResponse = JsonSerializer.Serialize(message);

    _mockHttpMessageHandler
      .When(ChannelMessagesEndpoint)
      .Respond(HttpStatusCode.OK, "application/json", messageResponse);

    var request = new CreateMessageRequest(
      "content",
      new(DiscordMessageReferenceTypes.Default, "message_id", "channel_id", "guild_id", false)
    );

    var result = await _discordRestClient.CreateMessageAsync("channel_id", request);

    result.Should().BeEquivalentTo(message);
  }

  public void Dispose()
  {
    _mockHttpMessageHandler.Dispose();
  }
}