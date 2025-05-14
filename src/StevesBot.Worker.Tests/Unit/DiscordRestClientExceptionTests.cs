namespace StevesBot.Worker.Tests.Unit;

public class DiscordRestClientExceptionTests
{
  [Fact]
  public void Constructor_WhenCalledWithNoParameters_ItShouldCreateInstance()
  {
    var exception = new DiscordRestClientException();

    exception.Should().NotBeNull();
  }

  [Fact]
  public void Constructor_WhenCalledWithMessage_ItShouldCreateInstance()
  {
    var message = "Test message";
    var exception = new DiscordRestClientException(message);

    exception.Should().NotBeNull();
    exception.Message.Should().Be(message);
  }

  [Fact]
  public void Constructor_WhenCalledWithMessageAndInnerException_ItShouldCreateInstance()
  {
    var message = "Test message";
    var innerException = new Exception("Inner exception");
    var exception = new DiscordRestClientException(message, innerException);

    exception.Should().NotBeNull();
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
  }
}