namespace StevesBot.Worker.Tests.Unit;

public class HeartbeatDiscordEventTests
{
  [Theory]
  [InlineData(null)]
  [InlineData(1)]
  public void Constructor_WhenCalled_ItShouldInitializeProperties(int? sequence)
  {
    var heartbeatEvent = new HeartbeatDiscordEvent(sequence);

    heartbeatEvent.OpCode.Should().Be(1);
    heartbeatEvent.Sequence.Should().Be(sequence);
    heartbeatEvent.Type.Should().BeNull();
    heartbeatEvent.Data.Should().BeNull();
  }
}