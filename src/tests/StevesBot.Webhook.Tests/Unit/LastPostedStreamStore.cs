namespace StevesBot.Webhook.Tests.Unit;

public class LastPostedStreamStoreTests
{
  [Fact]
  public void SetValue_WhenCalledWithNull_ItShouldThrowArgumentNullException()
  {
    var store = new LastPostedStreamStore();

    var act = () => store.SetValue(null!);

    act.Should().Throw<ArgumentNullException>();
  }

  [Fact]
  public void HasValue_WhenCalledWithNull_ItShouldThrowArgumentNullException()
  {
    var store = new LastPostedStreamStore();

    var act = () => store.HasValue(null!);

    act.Should().Throw<ArgumentNullException>();
  }


  [Fact]
  public void SetValueHasValue_WhenCalled_ItShouldStoreValue()
  {
    var store = new LastPostedStreamStore();
    var streamId = "test-stream-id";

    store.SetValue(streamId);

    store.HasValue(streamId).Should().BeTrue();
  }
}