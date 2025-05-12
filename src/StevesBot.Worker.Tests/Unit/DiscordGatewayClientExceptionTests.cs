namespace StevesBot.Worker.Tests.Unit;

public class DiscordGatewayClientExceptionTests
{
  [Fact]
  public void Constructor_WhenCalledWithoutParameters_ItShouldCreateInstance()
  {
    var exception = new DiscordGatewayClientException();

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordGatewayClientException>();
  }

  [Fact]
  public void Constructor_WhenCalledWithMessage_ItShouldCreateInstance()
  {
    var message = "Test message";
    var exception = new DiscordGatewayClientException(message);

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordGatewayClientException>();
    exception.Message.Should().Be(message);
  }

  [Fact]
  public void Constructor_WhenCalledWithMessageAndInnerException_ItShouldCreateInstance()
  {
    var message = "Test message";
    var innerException = new Exception("Inner exception");
    var exception = new DiscordGatewayClientException(message, innerException);

    exception.Should().NotBeNull();
    exception.Should().BeOfType<DiscordGatewayClientException>();
    exception.Message.Should().Be(message);
    exception.InnerException.Should().Be(innerException);
  }
}