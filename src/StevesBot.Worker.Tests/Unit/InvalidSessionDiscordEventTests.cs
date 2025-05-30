namespace StevesBot.Worker.Tests.Unit;

public class InvalidSessionDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldReturnAnInstance()
  {
    var result = new InvalidSessionDiscordEvent();

    result.OpCode.Should().Be(DiscordOpCodes.InvalidSession);
    result.Type.Should().BeNull();
    result.Sequence.Should().BeNull();
    result.Data.Should().BeFalse();
  }
}