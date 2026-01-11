namespace StevesBot.Webhook.Tests.Unit;

public class LastPostedStreamStoreTests
{
  private readonly Mock<TimeProvider> _mockTimeProvider = new();
  private readonly LastPostedStreamStore _sut;

  public LastPostedStreamStoreTests()
  {
    _sut = new(_mockTimeProvider.Object);
  }

  [Fact]
  public void SetValue_WhenCalledWithNull_ItShouldThrowArgumentNullException()
  {
    var act = () => _sut.SetValue(null!);

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void HasValue_WhenCalledWithNull_ItShouldThrowArgumentNullException()
  {
    var act = () => _sut.HasValue(null!);

    act.Should().Throw<ArgumentNullException>();
  }


  [Fact]
  public void SetValueHasValue_WhenCalled_ItShouldStoreValue()
  {
    var streamId = "test-stream-id";

    _sut.SetValue(streamId);

    _sut.HasValue(streamId).Should().BeTrue();
  }

  [Fact]
  public void RemoveValuesOlderThan24Hours_WhenCalled_ItShouldRemoveItemsOlderThan24Hours()
  {
    var oldStreamId = "test-stream-id-old";
    var currentStreamId = "test-stream-id-current";

    var currentTime = DateTimeOffset.UtcNow;
    var invalidTime = DateTimeOffset.UtcNow.AddHours(-25);
    var validTime = DateTimeOffset.UtcNow.AddHours(-10);

    _mockTimeProvider.SetupSequence(static m => m.GetUtcNow())
      .Returns(invalidTime)
      .Returns(validTime)
      .Returns(currentTime);

    _sut.SetValue(oldStreamId);
    _sut.SetValue(currentStreamId);

    _sut.RemoveValuesOlderThan24Hours();

    _sut.HasValue(oldStreamId).Should().BeFalse();
    _sut.HasValue(currentStreamId).Should().BeTrue();
  }
}