namespace StevesBot.Worker.Tests.Unit;

public class HelloDiscordEventTests
{
  [Fact]
  public void Constructor_WhenCalled_ItShouldInitializeProperties()
  {
    var helloEvent = new HelloDiscordEvent();
    var helloData = new HelloData();

    helloEvent.OpCode.Should().Be(0);
    helloEvent.Sequence.Should().BeNull();
    helloEvent.Type.Should().BeNull();
    helloEvent.Data.Should().BeEquivalentTo(helloData);
    helloData.HeartbeatInterval.Should().Be(0);
  }

  [Fact]
  public void Constructor_WhenCalledWithParameters_ItShouldInitializeProperties()
  {
    var opCode = 1;
    var sequence = 2;
    var type = "type";
    var heartbeatInterval = 1000;


    var helloData = new HelloData
    {
      HeartbeatInterval = heartbeatInterval
    };

    var helloEvent = new HelloDiscordEvent
    {
      OpCode = opCode,
      Sequence = sequence,
      Type = type,
      Data = helloData
    };

    helloEvent.OpCode.Should().Be(opCode);
    helloEvent.Sequence.Should().Be(sequence);
    helloEvent.Type.Should().Be(type);
    helloEvent.Data.Should().BeSameAs(helloData);
  }

  [Fact]
  public void Deserialize_WhenCalled_ItShouldReturnHelloDiscordEvent()
  {
    var data = new
    {
      op = 0,
      s = null as int?,
      t = null as string,
      d = new
      {
        heartbeat_interval = 1000
      }
    };

    var json = JsonSerializer.Serialize(data);

    var helloEvent = JsonSerializer.Deserialize<HelloDiscordEvent>(json);

    helloEvent.Should().NotBeNull();
    helloEvent!.OpCode.Should().Be(data.op);
    helloEvent.Sequence.Should().Be(data.s);
    helloEvent.Type.Should().Be(data.t);
    helloEvent.Data.Should().NotBeNull();
    helloEvent.Data!.HeartbeatInterval.Should().Be(data.d.heartbeat_interval);
  }
}