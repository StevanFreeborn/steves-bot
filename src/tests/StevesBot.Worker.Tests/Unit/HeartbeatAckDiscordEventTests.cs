namespace StevesBot.Worker.Tests.Unit;

public class HeartbeatAckDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new HeartbeatAckDiscordEvent();

    result.OpCode.Should().Be(DiscordOpCodes.HeartbeatAck);
    result.Sequence.Should().BeNull();
    result.Type.Should().BeNull();
    result.Data.Should().BeNull();
  }
}