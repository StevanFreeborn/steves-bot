namespace StevesBot.Library.Tests.Unit;

public class ServicesExtensionsTests
{
  private readonly DiscordClientOptions _discordClientOptions = new()
  {
    ApiUrl = "https://test.com",
    AppToken = "test_token",
  };
  private readonly ServiceCollection _services = new();

  public ServicesExtensionsTests()
  {
    _services.AddSingleton(_discordClientOptions);
  }

  [Fact]
  public void AddDiscordRestClient_WhenCalled_ItShouldAddDiscordRestClient()
  {
    _services.AddDiscordRestClient();

    var act = () => _services
      .BuildServiceProvider()
      .GetRequiredService<IDiscordRestClient>();

    act.Should().NotThrow();
  }
}