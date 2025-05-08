namespace StevesBot.Worker.Tests.Unit;

public class DiscordClientExceptionTests
{
  [Fact]
  public void Constructor_WhenCalledWithoutParameters_ItShouldCreateInstance()
  {
    var exception = new DiscordClientException();

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordClientException>();
  }

  [Fact]
  public void Constructor_WhenCalledWithMessage_ItShouldCreateInstance()
  {
    var message = "Test message";
    var exception = new DiscordClientException(message);

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordClientException>();
    exception.Message.Should().Be(message);
  }

  [Fact]
  public void Constructor_WhenCalledWithMessageAndInnerException_ItShouldCreateInstance()
  {
    var message = "Test message";
    var innerException = new Exception("Inner exception");
    var exception = new DiscordClientException(message, innerException);

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordClientException>();
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
  }
}