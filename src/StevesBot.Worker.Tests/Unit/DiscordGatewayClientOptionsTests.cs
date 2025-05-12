namespace StevesBot.Worker.Tests.Unit;

public class DiscordGatewayClientOptionsTests
{
  [Fact]
  public void Constructor_WhenCalledWithoutParameters_ItShouldCreateInstance()
  {
    var options = new DiscordGatewayClientOptions();

    options.Should().NotBeNull();
    options.Should().BeOfType<DiscordGatewayClientOptions>();
    options.ApiUrl.Should().Be(string.Empty);
    options.AppToken.Should().Be(string.Empty);
    options.Intents.Should().Be(0);
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldCreateInstance()
  {
    var apiUrl = "https://api.example.com";
    var appToken = "test-token";
    var intents = 123;

    var options = new DiscordGatewayClientOptions
    {
      ApiUrl = apiUrl,
      AppToken = appToken,
      Intents = intents
    };

    options.Should().NotBeNull();
    options.Should().BeOfType<DiscordGatewayClientOptions>();
    options.ApiUrl.Should().Be(apiUrl);
    options.AppToken.Should().Be(appToken);
    options.Intents.Should().Be(intents);
  }
}