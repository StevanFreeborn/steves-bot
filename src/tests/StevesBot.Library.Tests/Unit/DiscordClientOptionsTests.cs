namespace StevesBot.Library.Tests.Unit;

public class DiscordClientOptionsTests
{
  [Fact]
  public void Constructor_WhenCalledWithoutParameters_ItShouldCreateInstance()
  {
    var options = new DiscordClientOptions();

    options.Should().NotBeNull();
    options.Should().BeOfType<DiscordClientOptions>();
    options.ApiUrl.Should().Be(string.Empty);
    options.AppToken.Should().Be(string.Empty);
    options.Intents.Should().Be(0);
    
    // Verify default retry configuration
    options.MaxRetryAttempts.Should().Be(5);
    options.BaseRetryDelayMs.Should().Be(1000);
    options.MaxRetryDelayMs.Should().Be(60000);
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldCreateInstance()
  {
    var apiUrl = "https://api.example.com";
    var appToken = "test-token";
    var intents = 123;

    var options = new DiscordClientOptions
    {
      ApiUrl = apiUrl,
      AppToken = appToken,
      Intents = intents
    };

    options.Should().NotBeNull();
    options.Should().BeOfType<DiscordClientOptions>();
    options.ApiUrl.Should().Be(apiUrl);
    options.AppToken.Should().Be(appToken);
    options.Intents.Should().Be(intents);
  }

  [Fact]
  public void Constructor_WhenCalledWithCustomRetryOptions_ItShouldUseProvidedValues()
  {
    var options = new DiscordClientOptions
    {
      MaxRetryAttempts = 3,
      BaseRetryDelayMs = 500,
      MaxRetryDelayMs = 30000
    };

    options.MaxRetryAttempts.Should().Be(3);
    options.BaseRetryDelayMs.Should().Be(500);
    options.MaxRetryDelayMs.Should().Be(30000);
  }
}