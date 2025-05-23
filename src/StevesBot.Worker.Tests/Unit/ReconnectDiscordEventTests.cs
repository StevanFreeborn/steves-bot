namespace StevesBot.Worker.Tests.Unit;

public class ReconnectDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new ReconnectDiscordEvent();

    result.OpCode.Should().Be(DiscordOpCodes.Reconnect);
    result.Sequence.Should().BeNull();
    result.Type.Should().BeNull();
    result.Data.Should().BeNull();
  }
}