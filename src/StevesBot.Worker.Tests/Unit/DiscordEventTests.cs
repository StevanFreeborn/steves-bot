namespace StevesBot.Worker.Tests.Unit;

public class DiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldCreateInstance()
  {
    var e = new DiscordEvent();

    e.Should().NotBeNull();
    e.Should().BeOfType<DiscordEvent>();
    e.OpCode.Should().Be(0);
    e.Sequence.Should().BeNull();
    e.Type.Should().BeNull();
    e.Data.Should().BeNull();
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldCreateInstance()
  {
    var opCode = 1;
    var sequence = 2;
    var type = "test";
    var data = new object();

    var e = new DiscordEvent
    {
      OpCode = opCode,
      Sequence = sequence,
      Type = type,
      Data = data,
    };

    e.Should().NotBeNull();
    e.Should().BeOfType<DiscordEvent>();
    e.OpCode.Should().Be(opCode);
    e.Sequence.Should().Be(sequence);
    e.Type.Should().Be(type);
    e.Data.Should().BeSameAs(data);
  }
}