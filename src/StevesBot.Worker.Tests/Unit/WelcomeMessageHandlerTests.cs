using StevesBot.Worker.Handlers;

namespace StevesBot.Worker.Tests.Unit;

public class WelcomeMessageHandlerTests
{
  private readonly Mock<IDiscordRestClient> _mockDiscordRestClient = new();
  private readonly Mock<ILogger<IDiscordGatewayClient>> _mockLogger = new();
  private readonly IServiceProvider _serviceProvider;

  public WelcomeMessageHandlerTests()
  {
    var serviceCollection = new ServiceCollection();
    serviceCollection.AddSingleton(_mockDiscordRestClient.Object);
    serviceCollection.AddSingleton(_mockLogger.Object);

    _serviceProvider = serviceCollection.BuildServiceProvider();
  }

  [Fact]
  public async Task HandleAsync_WhenEventIsNotMessageCreateEvent_ItShouldNotCreateMessage()
  {
    await WelcomeMessageHandler.HandleAsync(new DiscordEvent(), _serviceProvider);

    _mockDiscordRestClient
      .Verify(
        static c => c.CreateMessageAsync(It.IsAny<string>(), It.IsAny<CreateMessageRequest>(), It.IsAny<CancellationToken>()),
        Times.Never
      );
  }
}