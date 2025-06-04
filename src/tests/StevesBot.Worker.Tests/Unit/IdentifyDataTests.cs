using StevesBot.Worker.Discord.Gateway.Events.Data;

namespace StevesBot.Worker.Tests.Unit;

public class IdentifyDataTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var result = new IdentifyData();

    result.Token.Should().Be(string.Empty);
    result.Properties.Should().BeEquivalentTo(new IdentifyProperties());
    result.Presence.Should().BeEquivalentTo(new UpdatePresenceData());
    result.Intents.Should().Be(0);
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldReturnInstance()
  {
    var token = "test_token";
    var properties = new IdentifyProperties
    {
      Os = "test_os",
      Browser = "test_browser",
      Device = "test_device",
    };

    var presence = new UpdatePresenceData
    {
      Status = "test_status",
      Activities = [],
    };

    var intents = 123456789;

    var result = new IdentifyData
    {
      Token = token,
      Properties = properties,
      Presence = presence,
      Intents = intents
    };

    result.Token.Should().Be(token);
    result.Properties.Should().BeSameAs(properties);
    result.Presence.Should().BeSameAs(presence);
    result.Intents.Should().Be(intents);
  }
}