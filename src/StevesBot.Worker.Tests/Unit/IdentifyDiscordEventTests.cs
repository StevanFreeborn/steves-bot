namespace StevesBot.Worker.Tests.Unit;

public class IdentifyDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnInstance()
  {
    var token = "test_token";
    var intents = 123456789;
    var presence = new UpdatePresenceData
    {
      Status = "online",
      Activities = [],
    };

    var result = new IdentifyDiscordEvent(token, intents, presence);

    result.OpCode.Should().Be(DiscordOpCodes.Identify);
    result.Data.Token.Should().Be(token);
    result.Data.Intents.Should().Be(intents);
    result.Data.Presence.Should().BeSameAs(presence);
  }
}