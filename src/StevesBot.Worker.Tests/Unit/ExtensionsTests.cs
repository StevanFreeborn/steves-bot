namespace StevesBot.Worker.Tests.Unit;

public class ExtensionsTests
{
  private readonly DiscordClientOptions _discordClientOptions = new()
  {
    ApiUrl = "https://test.com",
    AppToken = "test_token",
  };
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<ILogger<DiscordGatewayClient>> _mockLogger = new();
  private readonly Mock<IWebSocketFactory> _mockWebSocketFactory = new();
  private readonly Mock<TimeProvider> _mockTimeProvider = new();
  private readonly Mock<IServiceScopeFactory> _mockScopeFactory = new();

  private readonly ServiceCollection _services = new();

  public ExtensionsTests()
  {
    _services.AddSingleton(_discordClientOptions);
    _services.AddSingleton(_mockLogger.Object);
    _services.AddSingleton(_mockWebSocketFactory.Object);
    _services.AddSingleton(_mockTimeProvider.Object);
    _services.AddSingleton(_mockScopeFactory.Object);
  }

  [Fact]
  public void AddDiscordRestClient_WhenCalled_ItShouldAddDiscordRestClient()
  {
    _services.AddDiscordRestClient();

    var act = () => _services
      .BuildServiceProvider()
      .GetRequiredService<IDiscordRestClient>();

    act.Should().NotThrow();
  }

  [Fact]
  public void AddDiscordGatewayClient_WhenCalled_ItShouldAddDiscordGatewayClient()
  {
    _services.AddSingleton(_mockDiscordRestClient.Object);

    _services.AddDiscordGatewayClient();

    var act = () => _services
      .BuildServiceProvider()
      .GetRequiredService<IDiscordGatewayClient>();

    act.Should().NotThrow();
  }

  [Fact]
  public void AddDiscordGatewayClient_WhenCalledWithConfigureAction_ItShouldAddDiscordGatewayClientAndConfigureIt()
  {
    var mockAction = new Mock<Action<IDiscordGatewayClient>>();

    _services.AddSingleton(_mockDiscordRestClient.Object);

    _services.AddDiscordGatewayClient(mockAction.Object);

    var act = () => _services
      .BuildServiceProvider()
      .GetRequiredService<IDiscordGatewayClient>();

    act.Should().NotThrow();

    mockAction.Invocations.Count.Should().Be(1);
  }
}