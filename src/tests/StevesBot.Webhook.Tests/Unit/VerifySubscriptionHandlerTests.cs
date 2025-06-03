namespace StevesBot.Webhook.Tests.Unit;

public class VerifySubscriptionHandlerTests
{
  private readonly Mock<IOptions<SubscriptionOptions>> _mockSubOptions = new();
  private readonly Mock<ILogger<Program>> _mockLogger = new();
  private readonly ConcurrentQueue<SubscribeTask> _subscriptionQueue = new();

  [Fact]
  public void Handle_WhenModeIsDenied_ItShouldReturnBadRequest()
  {
    var result = Handle("denied", "topic");

    result.Should().BeOfType<BadRequest<string>>();
  }

  private IResult Handle(string mode, string topic, string? reason = null, string? challenge = null, string? leaseSeconds = null)
  {
    return VerifySubscriptionHandler.Handle(
      mode,
      topic,
      reason,
      challenge,
      leaseSeconds,
      _mockSubOptions.Object,
      _mockLogger.Object,
      _subscriptionQueue
    );
  }
}