using System.Globalization;

namespace StevesBot.Webhook.Tests.Unit;

public class VerifySubscriptionHandlerTests
{
  private readonly Mock<IOptions<SubscriptionOptions>> _mockSubOptions = new();
  private readonly Mock<ILogger<Program>> _mockLogger = new();
  private readonly ConcurrentQueue<SubscribeTask> _subscriptionQueue = new();
  private readonly Mock<TimeProvider> _mockTimeProvider = new();

  [Fact]
  public void Handle_WhenModeIsDenied_ItShouldReturnBadRequest()
  {
    var result = Handle("denied", "topic");

    result.Should().BeOfType<BadRequest<string>>();
  }

  [Fact]
  public void Handle_WhenTopicDoesNotMatch_ItShouldReturnNotFound()
  {
    SetupSubscriptionOptions(new() { TopicUrl = "expected-topic" });

    var result = Handle("subscribe", "wrong-topic");

    result.Should().BeOfType<NotFound>();
  }

  [Fact]
  public void Handle_WhenModeIsSubscribeAndLeaseSecondsIsNotPresent_ItShouldReturnBadRequest()
  {
    var topic = "topic";

    SetupSubscriptionOptions(new() { TopicUrl = topic });

    var result = Handle("subscribe", topic, leaseSeconds: null);

    result.Should().BeOfType<BadRequest<string>>();
  }

  [Fact]
  public void Handle_WhenModeIsSubscribeAndLeaseSecondsIsInvalid_ItShouldReturnBadRequest()
  {
    var topic = "topic";

    SetupSubscriptionOptions(new() { TopicUrl = topic });

    var result = Handle("subscribe", topic, leaseSeconds: "invalid");

    result.Should().BeOfType<BadRequest<string>>();
  }

  [Fact]
  public void Handle_WhenModeIsUnsubscribe_ItShouldReturnOk()
  {
    var topic = "topic";

    SetupSubscriptionOptions(new() { TopicUrl = topic });

    var result = Handle("unsubscribe", topic, challenge: "challenge");

    result.Should().BeOfType<ContentHttpResult>();
  }

  [Fact]
  public void Handle_WhenModeIsSubscribe_ItShouldEnqueueSubscriptionTask()
  {
    var challenge = "challenge";
    var topic = "topic";
    var leaseSeconds = 3600;
    var callbackUrl = "https://example.com/callback";
    var now = DateTime.UtcNow;

    _mockTimeProvider
      .Setup(static tp => tp.GetUtcNow())
      .Returns(now);

    SetupSubscriptionOptions(new() { TopicUrl = topic, CallbackUrl = callbackUrl });

    var result = Handle(
      "subscribe",
      topic,
      challenge: challenge,
      leaseSeconds: leaseSeconds.ToString(CultureInfo.InvariantCulture)
    );

    result.Should().BeOfType<ContentHttpResult>();
    result.As<ContentHttpResult>().ResponseContent.Should().Be(challenge);

    _subscriptionQueue.Count.Should().Be(1);

    var task = _subscriptionQueue.TryDequeue(out var dequeuedTask);

    task.Should().BeTrue();
    dequeuedTask.Should().NotBeNull();
    dequeuedTask!.CallbackUrl.Should().Be(callbackUrl);
    dequeuedTask.TopicUrl.Should().Be(topic);
    dequeuedTask.ExpiresAt.Should().Be(now.AddSeconds(leaseSeconds));
  }

  private void SetupSubscriptionOptions(SubscriptionOptions options)
  {
    _mockSubOptions
      .Setup(static o => o.Value)
      .Returns(options);
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
      _subscriptionQueue,
      _mockTimeProvider.Object
    );
  }
}